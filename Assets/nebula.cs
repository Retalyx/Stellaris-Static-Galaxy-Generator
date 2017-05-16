using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class nebula : MonoBehaviour {
	public bool isMoving = false;
	public bool isHovered = false;
	public bool isSelected = false;
	public string nebulaName = "Nebula";
	public int nebulaRadius;
	public TextMesh nameText;
	Renderer mat;
	
	// Use this for initialization
	void Start () {
		mat = GetComponent<Renderer>();
		main.galaxynebulacount++;
		GameObject territory = this.transform.FindChild("territory").gameObject;
		territory.SetActive(true);
		territory.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
		territory.GetComponent<Renderer>().material.color = new Vector4(1, 0, 1, 0.5f);
		if(nebulaRadius <= 0)
			nebulaRadius = Random.Range(30,60);
		this.gameObject.name = "neb_" + nebulaName;
	}
	
	// Update is called once per frame
	void Update () {
		if(isMoving)
		{
			mat.material.color = Color.yellow;
			Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 460));
			this.transform.position = new Vector3(pos.x, 0, pos.z);
		}
		else
		{
			if(isHovered)
			{
					mat.material.color = Color.blue;
			}
			else
			{
				mat.material.color = Color.magenta;
				if(isSelected)
				{
					mat.material.color = Color.green;
				}
			}
		}
		if(isHovered || main.showTexts)
		{
			nameText.text = nebulaName;
		}
		else
		{
			nameText.text = "";
		}
		GameObject territory = this.transform.FindChild("territory").gameObject;
		territory.transform.localScale = new Vector3(nebulaRadius/2,0.1f,nebulaRadius/2);
	}
	
	void OnDestroy()
	{
		main.galaxynebulacount--;		
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
			switch(main.currentMode)
			{
				case 2:
					isMoving = true;
					break;
			}
		}
	}
	
	// public void setSpawn(bool set)
	// {
		// isSpawn = set;
		// territory.SetActive(isSpawn);
		// territory.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
		// territory.GetComponent<Renderer>().material.color = new Vector4(Random.value, Random.value, Random.value, 0.25f);
	// }
	
	void OnMouseUpAsButton()
	{
		if(GUIUtility.hotControl == 0)
		{
			switch(main.currentMode)
			{				
				case 2:
					isMoving = false;
					break;
					
				case 6:
					Destroy(this.gameObject);
					break;
				
				default:
					select(true);
					break;
			}
		}
		this.gameObject.name = "neb_" + nebulaName;
	}
	
	public void select(bool status)
	{
		if(status)
		{
			if(main.prevSelectedNebula)
			{
				main.prevSelectedNebula.select(false);
			}
			if(main.prevSelectedSystem)
			{
				main.prevSelectedSystem.select(false);
			}
			if(main.selectedSystem)
			{
				main.selectedSystem.select(false);
			}
			main.prevSelectedNebula = this;
			main.selectedNebula = this;
			isSelected = true;
		}
		else
		{
			isSelected = false;
		}
	}
}
