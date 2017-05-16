using UnityEngine;
using System.Collections;

public class hyperlane : MonoBehaviour {
	public system link0;
	public system link1;
	public bool prevent = false;
	
	// Use this for initialization
	void Start () {
		prevent = main.hlpreventmode;
		SetName();
	}
	
	// Update is called once per frame
	void Update () {
		if(!link0 || !link1)
		{
			Destroy(this.gameObject);
		}
		else
		{
			this.gameObject.GetComponent<LineRenderer>().SetPosition(0,link0.transform.position);
			this.gameObject.GetComponent<LineRenderer>().SetPosition(1,link1.transform.position);
		}
		if(prevent)
		{
			this.gameObject.GetComponent<LineRenderer>().SetColors(Color.red, Color.red);
		}
		else
		{
			this.gameObject.GetComponent<LineRenderer>().SetColors(Color.gray, Color.gray);
		}
	}
	
	public void SetName()
	{
		this.gameObject.name = "hl_" + link0.systemid + "_to_" + link1.systemid;
	}
	
	public void kill()
	{
		this.gameObject.name = "DestroyedLane";
		if(link0)
		{
			link0.connectedLines--;
		}		
		if(link1)
		{
			link1.connectedLines--;
		}	
		
		Destroy(this.gameObject);
	}
}
