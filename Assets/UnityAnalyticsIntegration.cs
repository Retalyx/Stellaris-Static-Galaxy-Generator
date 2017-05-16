using UnityEngine;
using System.Collections;
using UnityEngine.Cloud.Analytics;

public class UnityAnalyticsIntegration : MonoBehaviour {

    // Use this for initialization
    void Start () {

        const string projectId = "293d2c1a-d7c4-4aa2-9985-5d3b809d7ff8";
        UnityAnalytics.StartSDK (projectId);

    }

}