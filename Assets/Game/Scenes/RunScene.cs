using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunScene : MonoBehaviour
{
    [SerializeField]
    private PathCreation.PathCreator routePath = null;

    [SerializeField]
    private GameObject pipePrefab = null;

    [SerializeField]
    private Transform cameraTransform = null;

    // Start is called before the first frame update
    void Start()
    {
        

        /*
        var points = this.routePath.path.localPoints;

        for (int i = 1; i < points.Length; i++)
        {
            var v1 = points[i - 1];
            var v2 = points[i];
            var magnitude = (v2 - v1).magnitude;

            var x = 22.5f * magnitude;

            var pipe = Instantiate(this.pipePrefab);
            pipe.transform.localPosition = v2 * 22.5f;
            pipe.transform.forward = v2 - v1;
            pipe.transform.localScale = new Vector3(1f, 1f, x);

        }
        */

        //10m : 45s
        //5b : 1s
        //5b x 45s = 225b
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
