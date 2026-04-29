using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public struct MoveData : IReplicateData
    {
        public float Horizontal;
        public float Vertical;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
    
    public class PlayerMovementPredicted : NetworkBehaviour
    {
        [SerializeField] private float _speed = 5f;
        
        private InputSystem_Actions _input;
        private Vector2 _moveVector;

        public override void OnStartNetwork()
        {
            TimeManager.OnTick += OnTick;
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnTick -= OnTick;
        }
        
        private void Awake()
        {
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.Player.Move.performed += OnMovementStart;
            _input.Player.Move.canceled += OnMovementStop;
        }

        private void OnDisable()
        {
            _input.Player.Move.performed -= OnMovementStart;
            _input.Player.Move.canceled -= OnMovementStop;
            _input.Disable();
        }
        
        private void OnMovementStart(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            _moveVector = context.ReadValue<Vector2>();
        }
        
        private void OnMovementStop(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            _moveVector = Vector2.zero;
        }


        private void OnTick()
        {
            // Владелец собирает ввод и отправляет на сервер.
            if (IsOwner)
            {
                MoveData md = new MoveData
                {
                    Horizontal = _moveVector.x,
                    Vertical = _moveVector.y
                };
                Replicate(md);
            }
            else
            {
                Replicate(default);
            }

            // Сервер периодически шлёт «истинное» состояние для коррекции.
            if (IsServerInitialized)
            {
                ReconcileData rd = new ReconcileData
                {
                    Position = transform.position
                };
                Reconcile(rd);
            }
        }

        [Replicate]
        private void Replicate(
            MoveData md,
            ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized;
            transform.position += move * _speed * (float)base.TimeManager.TickDelta;
        }

        public override void CreateReconcile()
        {
            ReconcileData rd = new ReconcileData
            {
                Position = transform.position
            };
    
            // Отправляем на реконсиляцию
            Reconcile(rd);
        }

        [Reconcile]
        private void Reconcile(
            ReconcileData rd,
            Channel channel = Channel.Unreliable)
        {
            transform.position = rd.Position;
        }
    }
}