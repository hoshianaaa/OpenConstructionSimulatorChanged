using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
//using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;

public class PosePub : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "pose";

    private PoseStampedMsg message;

    // Publish the cube's position and rotation every N seconds
    public float publishMessageInterval = 0.5f;//2Hz

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    // Start is called before the first frame update
    void Start()
    {
        message = new PoseStampedMsg();
        message.header = new RosMessageTypes.Std.HeaderMsg();
        message.header.stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg();

        ros = ROSConnection.instance;
        ros.RegisterPublisher<PoseStampedMsg>(topicName);
    }

    // Update is called once per constant rate
    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= publishMessageInterval){
            float sim_time = Time.time;
            uint secs = (uint)sim_time;
            uint nsecs = (uint)((sim_time % 1) * 1e9);
            message.header.frame_id = "gps";
            message.header.stamp.sec = secs;
            message.header.stamp.nanosec = nsecs;

            message.pose.position.x = this.transform.position.z;
            message.pose.position.y = - this.transform.position.x;
            message.pose.position.z = this.transform.position.y;

            message.pose.orientation.x = - this.transform.rotation.z;
            message.pose.orientation.y = this.transform.rotation.x;
            message.pose.orientation.z = - this.transform.rotation.y;
            message.pose.orientation.w = this.transform.rotation.w;

            ros.Send(topicName, message);
            timeElapsed = 0.0f;
        }
    }
}
