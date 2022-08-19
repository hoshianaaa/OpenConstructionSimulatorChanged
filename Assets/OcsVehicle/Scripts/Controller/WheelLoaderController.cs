using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.Robotics.ROSTCPConnector;
using Float64 = RosMessageTypes.Std.Float64Msg;
using Bool = RosMessageTypes.Std.BoolMsg;

namespace Ocs.Vehicle.Controller
{
    public class WheelLoaderController : MonoBehaviour
    {
        [SerializeField] private WheelLoader _vehicle;
        private Ocs.Input.VehicleInput _input;
        [Header("- Topic Name -")]
        [SerializeField] private string wheelDrive_topic = "wheelLoader/wheel";
        [SerializeField] private string steer_topic = "wheelLoader/steer";
        [SerializeField] private string boom_topic = "wheelLoader/boom";
        [SerializeField] private string bucket_topic = "wheelLoader/bucket";
        [SerializeField] private string reverse_gear_topic = "wheelLoader/reverse_gear";

        private float wheel_input, steer_input, boom_input, bucket_input;
        private bool reverse_gear;

        [SerializeField] private ModeManeger mode;


        private void Awake()
        {
            this._input = new Ocs.Input.VehicleInput();
        }

        private void Start()
        {
            // Callback
            this._input.Car.ShiftUp.started += context => this._vehicle.ReverseGear = false;
            this._input.Car.ShiftDown.started += context => this._vehicle.ReverseGear = true;
            this._input.Equipment.Light.started += context => this._vehicle.SwitchLight();
            this._input.Equipment.Hone.started += context => this._vehicle.PlayHone();
            this._input.Equipment.LeftWinker.started += context => this._vehicle.SwitchLeftWinker();
            this._input.Equipment.RightWinker.started += context => this._vehicle.SwitchRightWinker();

            //ros
            ROSConnection.GetOrCreateInstance().Subscribe<Float64>(this.wheelDrive_topic, wheel_callback);
            ROSConnection.GetOrCreateInstance().Subscribe<Float64>(this.steer_topic, steer_callback);
            ROSConnection.GetOrCreateInstance().Subscribe<Float64>(this.boom_topic, boom_callback);
            ROSConnection.GetOrCreateInstance().Subscribe<Float64>(this.bucket_topic, bucket_callback);
            ROSConnection.GetOrCreateInstance().Subscribe<Bool>(this.reverse_gear_topic, reverse_gear_callback);

        }

        private void OnEnable() => this._input.Enable();
        private void OnDestroy() => this._input.Dispose();

        private void OnDisable()
        {
            this._input.Disable();
            this._vehicle.AccelInput = 0.0f;
            this._vehicle.BrakeInput = 1.0f;
            this._vehicle.SteerInput = 0.0f;
        }

        void Update()
        {
            if(!mode.Automation){
                this._vehicle.AccelInput = this._input.Car.Accel.ReadValue<float>();
                this._vehicle.BrakeInput = this._input.Car.Brake.ReadValue<float>();
                this._vehicle.SteerInput = this._input.Car.Steering.ReadValue<Vector2>()[0];
                this._vehicle.BoomInput = -this._input.Backhoe.Lever1.ReadValue<Vector2>()[1];
                this._vehicle.BucketInput = this._input.Backhoe.Lever1.ReadValue<Vector2>()[0];
            }else{
                this._vehicle.AccelInput = wheel_input;
                this._vehicle.BrakeInput = 0;
                if(wheel_input < 0){
                    this._vehicle.AccelInput = 0;
                    this._vehicle.BrakeInput = System.Math.Abs(wheel_input);
                }
                //this._vehicle.BrakeInput = ;
                this._vehicle.SteerInput = steer_input;
                this._vehicle.BoomInput = boom_input;
                this._vehicle.BucketInput = bucket_input;
                this._vehicle.ReverseGear = reverse_gear;

            }
        }

        void wheel_callback(Float64 wheel_message)
        {
            wheel_input = (float)wheel_message.data;
        }

        void steer_callback(Float64 steer_message)
        {
            steer_input = (float)steer_message.data;
        }

        void boom_callback(Float64 boom_message)
        {
            boom_input = (float)boom_message.data;
        }

        void bucket_callback(Float64 bucket_message)
        {
            bucket_input = (float)bucket_message.data;
        }
        void reverse_gear_callback(Bool reverse_gear_message)
        {
            reverse_gear = (bool)reverse_gear_message.data;
        }

    }
}
