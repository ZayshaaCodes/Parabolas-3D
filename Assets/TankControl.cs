using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class TankControl : MonoBehaviour, Parabolas3D.IPlayerActions
{
    private Parabolas3D _input;

    public Transform barrel;
    public VisualEffect barrelVfx;
    public LayerMask groundMask;
    public Projectile projectilePrefab;
    public int gravity = 5;

    private Vector2 _moveVec;
    private Vector2 _curMouseScreenPoint;
    private Rigidbody2D _rb;
    private CircleCollider2D _coll;
    private Collider2D[] _overlapped;
    private int _rotateToGroundSpeed = 360;
    private bool _grounded = false;
    public float moveForce = 5;
    public float shootImpulseForce = 9;

    private void Start()
    {
        _input = new Parabolas3D();

        _input.Enable();
        _input.Player.SetCallbacks(this);

        _rb   = GetComponent<Rigidbody2D>();
        _coll = GetComponent<CircleCollider2D>();

        _overlapped = new Collider2D[20];
    }

    private void Update()
    {
        Vector2 groundForceDir = Vector2.zero;
        var     count          = Physics2D.OverlapCircleNonAlloc(transform.position, _coll.radius + .1f, _overlapped, groundMask);
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                var c = _overlapped[i];
                if (c.gameObject == gameObject) continue;

                var d = Physics2D.Distance(_coll, c);

                var n = d.isOverlapped ? d.normal : -d.normal;

                groundForceDir += n;


                // Debug.DrawLine(d.pointA, d.pointB, Color.red, 1);
                // Debug.DrawRay(d.pointB, n, d.isOverlapped ? Color.cyan : Color.green, 1);
            }

            groundForceDir.Normalize();
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                          Quaternion.LookRotation(Vector3.forward, -groundForceDir),
                                                          _rotateToGroundSpeed * Time.deltaTime);
            
            _grounded = true;            
            _rb.angularDrag = 3;
            _rb.drag        = 3;
            _rb.AddRelativeForce(new Vector2(_moveVec.x, 0) * moveForce, ForceMode2D.Force);
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            _grounded       = false;
            _rb.angularDrag = .1f;
            _rb.drag        = 0;
            _rb.constraints = RigidbodyConstraints2D.None;
            groundForceDir  = Vector2.down;
        }

        _rb.AddForce(gravity * _rb.mass * groundForceDir.normalized);
        // Debug.DrawRay(transform.position, groundForceDir.normalized, new Color(1f, 0.92f, 0.02f, 0.23f), 1);

        var mainCam = Camera.main!;
        var worldPos = mainCam.ScreenToWorldPoint(new Vector3(_curMouseScreenPoint.x,
                                                              _curMouseScreenPoint.y,
                                                              -mainCam.transform.position.z));

        var delta = worldPos - transform!.position;
        var angle = -math.degrees(math.atan2(delta.y, delta.x));
        barrel.rotation = Quaternion.Euler(angle, 90, 0);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveVec = context.ReadValue<Vector2>();

        // Debug.Log(val);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _curMouseScreenPoint = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            barrel.GetComponent<Animation>().Play();
            var newProj = Instantiate(projectilePrefab);
            newProj.transform.position = barrel.transform.position + barrel!.transform.forward * .5f;
            newProj.Shoot(barrel.forward);
            if (barrelVfx) barrelVfx.Play();
            
            var tankRb = GetComponent<Rigidbody2D>();
            Physics2D.IgnoreCollision(tankRb.GetComponent<Collider2D>(), newProj.GetComponent<Collider2D>());
            
            if (!_grounded)
            {
                tankRb.velocity        = Vector2.zero;
                tankRb.angularVelocity = 0;
                tankRb.AddForce(-barrel!.forward * shootImpulseForce, ForceMode2D.Impulse);
            }

            tankRb.AddTorque(Random.Range(-5, 5));
        }
    }
}