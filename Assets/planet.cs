using UnityEngine;
using System.Collections;

public class planet : MonoBehaviour {
	public bool isMoving = false;
	public bool isHovered = false;
	public bool isSelected = false;
	public Material orbitTexture;
	GameObject centeraxe;
	GameObject ownOrbitGO;
	CircleDrawer orbit;
	GameObject systemedit;
	systemeditor systemeditor;
	Renderer mat;
	public int orbitDistance = 10;
	public int orbitAngle = 0;
	public int planetSize = 0;
	public GameObject nameText;
	
	public string planetClass = "random";
	public string planetName = "random";
	public string planetModifier = "random";
	public int ownerSystem;
	public int planetid;
	public planet ownerPlanet;
	public bool isHome;
	public bool hasRing;
	public bool noAnomaly = false;
	public bool noResources = false;
	// Use this for initialization
	void Start () {
		mat = GetComponent<Renderer>();
		ownOrbitGO = new GameObject();
		ownOrbitGO.transform.SetParent(this.transform, true);
		ownOrbitGO.name = "orbit";
		ownOrbitGO.gameObject.AddComponent<LineRenderer>();
		ownOrbitGO.gameObject.AddComponent<CircleDrawer>();
		ownOrbitGO.GetComponent<Renderer>().material = orbitTexture;
		orbit = ownOrbitGO.gameObject.GetComponent<CircleDrawer>();
		centeraxe = (GameObject)GameObject.Find("CenterAxe");
		systemedit = (GameObject)GameObject.Find("SystemEditor_"+ownerSystem);
		systemeditor = systemedit.GetComponent<systemeditor>();
		// systemeditor.planets.Add(this.gameObject);
		if(ownerPlanet)
		{
			orbitDistance = (int)Mathf.Round(Vector3.Distance(this.transform.position, ownerPlanet.transform.position));
		}
		else
		{
			orbitDistance = (int)Mathf.Round(Vector3.Distance(this.transform.position, centeraxe.transform.position));
		}
		planetid = main.planetid;
		main.planetid++;
	}
	
	// Update is called once per frame
	void Update () {
		if(isMoving)
		{
			mat.material.color = Color.yellow;
			Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 460));
			this.transform.position = new Vector3(pos.x, 0, pos.z);
			if(ownerPlanet)
			{
				this.transform.LookAt(ownerPlanet.transform);
				orbitDistance = (int)Mathf.Round(Vector3.Distance(this.transform.position, ownerPlanet.transform.position));
			}
			else
			{
				this.transform.LookAt(centeraxe.transform);
				orbitDistance = (int)Mathf.Round(Vector3.Distance(this.transform.position, centeraxe.transform.position));
			}
		}
		else
		{
			if(isHovered)
			{
				mat.material.color = Color.blue;
			}
			else if(isSelected)
			{
				mat.material.color = Color.green;
			}
			else
			{
				mat.material.color = Color.white;
			}
			if(ownerPlanet)
			{
				this.transform.LookAt(ownerPlanet.transform);
				if(Vector3.Distance(this.transform.position, ownerPlanet.transform.position) != orbitDistance)
				{
					transform.position = (transform.position - ownerPlanet.transform.position).normalized * orbitDistance + ownerPlanet.transform.position;
				}
			}
			else
			{
				this.transform.LookAt(centeraxe.transform);
				if(Vector3.Distance(this.transform.position, centeraxe.transform.position) != orbitDistance)
				{
					transform.position = (transform.position - centeraxe.transform.position).normalized * orbitDistance + centeraxe.transform.position;
				}
			}
		}
		orbit.radius = orbitDistance;
        Vector3 targetDir;
        Vector3 forward = Vector3.left;
		if(ownerPlanet)
		{
			// forward = ownerPlanet.transform.forward;
			targetDir = ownerPlanet.transform.position - this.transform.position;
			orbit.offset = ownerPlanet.transform.position;
		}
		else
		{
			targetDir = centeraxe.transform.position - this.transform.position;
			orbit.offset = new Vector3(0,0,0);
		}
		int ang = (int)Mathf.Round(Vector3.Angle(targetDir, forward));
		if(ownerPlanet)
		{
			Vector3 localPos = this.transform.position - ownerPlanet.transform.position;
			if(localPos.z <= 0)
			{
				orbitAngle = 360-ang;
			}
			else if(localPos.z > 0)
			{
				orbitAngle = ang;
			}
			// orbitAngle += ownerPlanet.orbitAngle;
			// if(orbitAngle>360)
			// {
				// orbitAngle = orbitAngle - 360;
			// }
		}
		else
		{
			if(this.transform.position.z <= 0)
			{
				orbitAngle = 360-ang;
			}
			else if(this.transform.position.z > 0)
			{
				orbitAngle = ang;
			}
		}
		if(ownerPlanet)
		{
			if(planetSize <= 0)
			{
				this.transform.localScale = new Vector3(15/3,15/3,15/3);
			}
			else
			{
				this.transform.localScale = new Vector3(15/3,15/3,15/3);
				// this.transform.localScale = new Vector3(planetSize*1.5f,planetSize*1.5f,planetSize*1.5f);
			}
		}
		else
		{
			if(planetSize <= 0)
			{
				this.transform.localScale = new Vector3(20/3,20/3,20/3);
			}
			else
			{
				this.transform.localScale = new Vector3(20/3,20/3,20/3);
				// this.transform.localScale = new Vector3(planetSize/3,planetSize/3,planetSize/3);
			}
		}
		if((isHovered || main.showTexts) && planetName != "RandomName" && planetName != "random")
		{
			nameText.GetComponent<TextMesh>().text = planetName;
			nameText.transform.localEulerAngles = new Vector3(90,(360-this.transform.localEulerAngles.y)+180,0);
		}
		else
		{
			nameText.GetComponent<TextMesh>().text = "";
		}
		if(isHome)
		{
			planetClass = "ideal_planet_class";
			planetSize = 16;
			planetModifier = "none";
			noAnomaly = true;
			noResources = true;
		}
	}	
	
	void OnMouseEnter()
	{
		isHovered = true;
	}
	
	void OnMouseExit()
	{
		isHovered = false;
	}
	
	void OnMouseDown()
	{
		if(GUIUtility.hotControl == 0)
		{
			switch(systemeditor.currentMode)
			{
				case 1:
					isMoving = true;
					break;
			}
		}
	}
	
	void OnMouseUpAsButton()
	{
		if(GUIUtility.hotControl == 0)
		{
			switch(systemeditor.currentMode)
			{
				case 1:
					isMoving = false;
					break;
					
				case 3:
					Destroy(this.gameObject);
					break;
				
				default:
					select(true);
					break;
			}
		}
	}
	
	public void select(bool status)
	{
		if(status)
		{
			if(systemeditor.prevSelectedPlanet)
			{
				systemeditor.prevSelectedPlanet.select(false);
			}
			systemeditor.prevSelectedPlanet = this;
			systemeditor.selectedPlanet = this;
			isSelected = true;
		}
		else
		{
			isSelected = false;
		}
	}
	
	void OnDestroy()
	{
		systemeditor.planets.Remove(this.gameObject);
	}
}
