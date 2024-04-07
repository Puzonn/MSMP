using BepInEx.Logging;
using Msmp.Server;
using MyBox;
using System;
using UnityEngine;

namespace Msmp.Client.Controllers
{
    internal class MovementController
    {
        private Vector3 _lastPosition = Vector3.zero;
        private float _movementThreshold = 0.1f;

        private float _lastRotation = 0f;
        private float _rotationThreshold = 3f;

        private readonly ManualLogSource _logger;

        private PlayerController _playerController
        {
            get
            {
               return Singleton<PlayerController>.Instance;
            }
        }

        private readonly MsmpClient _client;

        public MovementController(ManualLogSource logger, MsmpClient client)
        {
            _logger = logger;
            _client = client;
        }

        private bool _ready 
        { 
            get 
            { 
                return _client != null && _client.Connected && _playerController != null; 
            }
        }

        public void OnUpdate()
        {
            if (_ready)
            {
                Vector3 currentPosition = _playerController.transform.position;

                float distanceTraveled = Vector3.Distance(currentPosition, _lastPosition);

                if (distanceTraveled > _movementThreshold)
                {
                    int intX = (int)Math.Round(currentPosition.x * 32.0);
                    int intY = (int)Math.Round(currentPosition.y * 32.0);
                    int intZ = (int)Math.Round(currentPosition.z * 32.0);

                    byte[] data = new byte[4 * 3];

                    Buffer.BlockCopy(BitConverter.GetBytes(intX), 0, data, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(intY), 0, data, 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(intZ), 0, data, 8, 4);

                    _lastPosition = _playerController.transform.position;
                    
                    Packet packet = new Packet(PacketType.PlayerMovement, data);

                    _client.SendPayload(packet);
                }

                float currentRotation = _playerController.transform.eulerAngles.y;
                float rotationDifference = Mathf.Abs(currentRotation - _lastRotation);

                if (rotationDifference > _rotationThreshold)
                {
                    int euler = (int)Math.Round(currentRotation * 32.0);

                    byte[] data = new byte[4];

                    Buffer.BlockCopy(BitConverter.GetBytes(euler), 0, data, 0, 4);

                    Packet packet = new Packet(PacketType.PlayerRotate, data);

                    _client.SendPayload(packet);

                    _lastRotation = currentRotation;
                }
            }
        }
    }
}
