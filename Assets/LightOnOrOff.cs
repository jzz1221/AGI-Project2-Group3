using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOnOrOff : MonoBehaviour
{
    Light roomLight;
    // Start is called before the first frame update
    void Start()
    {
        roomLight = gameObject.GetComponentInChildren<Light>();
        roomLight.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(TurnOnLight());
    }

    private IEnumerator TurnOnLight() {
        yield return new WaitForSeconds(3f);
        roomLight.enabled = true;
    }

    
}
