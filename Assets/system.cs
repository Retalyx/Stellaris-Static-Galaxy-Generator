using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class system : MonoBehaviour {
	public GameObject hyperlane_prefab;
	public TextMesh nameText;
	public bool isSpawn = false;
	public bool isMoving = false;
	public bool isHovered = false;
	public bool isSelected = false;
	public int systemid;
	public int connectedLines = 0;
	public string systemName = "random";
	public bool prevent = false;
	public string initializer;
	Renderer mat;
	
	// Use this for initialization
	void Start () {
		mat = GetComponent<Renderer>();
		main.galaxysystemcount++;
		systemid = main.systemid;
		main.systemid++;
		this.gameObject.name = "sys_" + systemid;
		main.systems.Add(this);
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
				if(isSpawn)
				{
					mat.material.color = Color.red;
				}
				else
				{
					if(prevent)
					{
						mat.material.color = new Vector4(0.25f, 0.25f, 0.25f, 1f);
					}
					else
					{
						mat.material.color = Color.white;
					}
					if(initializer != "" && initializer != null)
					{
						mat.material.color = Color.magenta;
					}
				}
				if(isSelected || main.prevSelectedHyperlane == this)
				{
					mat.material.color = Color.green;
				}
			}
		}
		if((isHovered || main.showTexts) && systemName != "RandomName" && systemName != "random")
		{
			nameText.text = systemName;
		}
		else
		{
			nameText.text = "";
		}
	}
	
	void OnDestroy()
	{
		main.galaxysystemcount--;		
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
	
	public void setSpawn(bool set)
	{
		isSpawn = set;
		GameObject territory = this.transform.FindChild("territory").gameObject;
		territory.SetActive(isSpawn);
		territory.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
		territory.GetComponent<Renderer>().material.color = new Vector4(Random.value, Random.value, Random.value, 0.25f);
		if(isSpawn)
			main.spawns++;
		else
			main.spawns--;
	}
	
	void OnMouseUpAsButton()
	{
		if(GUIUtility.hotControl == 0)
		{
			switch(main.currentMode)
			{
				case 1:
					setSpawn(!isSpawn);
					break;
					
				case 2:
					isMoving = false;
					break;
					
				case 4:
					Destroy(this.gameObject);
					break;
						
				case 7:
					if(main.prevSelectedHyperlane)
					{
						if(main.prevSelectedHyperlane != this)
						{
							// Debug.Log("creating hyperlane between system "+main.prevSelectedHyperlane.systemid+" and system "+systemid);
							GameObject line = (GameObject)Instantiate(hyperlane_prefab, new Vector3(0, 0, 0), Quaternion.identity);
							hyperlane hl = line.GetComponent<hyperlane>();
							line.transform.SetParent(main.GalaxyEditor.transform, true);
							hl.link0 = main.prevSelectedHyperlane;
							hl.link1 = this;
							hl.transform.position = main.prevSelectedHyperlane.transform.position;
							main.prevSelectedHyperlane.connectedLines++;
							this.connectedLines++;
							main.prevSelectedHyperlane  = null;
						}
					}
					else
					{
						main.prevSelectedHyperlane = this;
					}
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
			if(main.prevSelectedSystem)
			{
				main.prevSelectedSystem.select(false);
			}
			if(main.prevSelectedNebula)
			{
				main.prevSelectedNebula.select(false);
			}
			if(main.selectedNebula)
			{
				main.selectedNebula.select(false);
			}
			main.prevSelectedSystem = this;
			main.selectedSystem = this;
			isSelected = true;
		}
		else
		{
			isSelected = false;
		}
	}
}
