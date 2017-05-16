using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Cloud.Analytics;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using SimpleJSON;

public class main : MonoBehaviour {
	public static string version = "0.9";
	public static string supportedGameVersion = "1.2.*";
	public GameObject system_prefab;
	public GameObject nebula_prefab;
	public GameObject hyperlane_prefab;
	public GameObject systemeditor_prefab;
	public Transform CameraAxe;
	public Transform center;
	public static LineRenderer lineUp, lineDown, lineLeft, lineRight;
	public static string galaxyname = "Random Generated Galaxy";
	public ProjectManager projectManager;
	public static string generationStatusString = "Not Generating";
	string lv;
	string lvdownload;
	int pnggalaxyX = 0;
	int pnggalaxyY = 0;
	int genspawn = 1;
	
	public static int genhlradius = 25;
	public static int genhlmaxlines = 4;
	public static float galaxysize = 600;
	public static float generationseed = 1234;
	public static float starstogen = 1000f;
	public static float nscale = 100f;
	public static float nfreq = 0.1f;
	public static float npers = 0.6f;
	public static float pngmultiplier = 1f;
	public static float pngrandomizer = 0f;
	public static float pngreducer = 0f;
	public static bool usePerlin = false;
	
	bool loadfromfile = false;
	public static GameObject GalaxyEditor;
    public static string message;
	public static int systemid = 0;
	public static int galaxysystemcount;
	public static int galaxynebulacount;
	public static int currentMode = 0;
	public static int editorMode = 0;
	public static int spawns = 0;
	public static int minDist = 12;
	public static int planetid = 0;
	public static bool hlpreventmode = false;
	public static bool isGenerating = false;
	public static bool showTexts = false;
	public static bool autogenHyperlines = true;
	public static system selectedSystem;
	public static system prevSelectedSystem;
	public static system prevSelectedHyperlane;
	public static nebula selectedNebula;
	public static nebula prevSelectedNebula;
	public static List<GameObject> systemEditors = new List<GameObject>();
	public static List<system> systems = new List<system>();
	public static Vector2 scrollPos = Vector2.zero;
	public static Rect scrollerRect = new Rect(10, 10, 200, 500);
	
	public static List<string> planetClassPresets = new List<string>();
	public static List<string> planetClassPresetsN = new List<string>();
	public static List<string> starClassPresets = new List<string>();
	public static List<string> starClassPresetsN = new List<string>();
	public static List<string> planetModifierPresets = new List<string>();
	public static List<string> planetModifierPresetsN = new List<string>();
	
	// Use this for initialization
	void Start () {
		GalaxyEditor = GameObject.Find("GalaxyEditor");
		lineUp = GameObject.Find("lineUp").GetComponent<LineRenderer>();
		lineDown = GameObject.Find("lineDown").GetComponent<LineRenderer>();
		lineLeft = GameObject.Find("lineLeft").GetComponent<LineRenderer>();
		lineRight = GameObject.Find("lineRight").GetComponent<LineRenderer>();
		StartCoroutine(CheckVersion());
		loadPresets();
	}
	
	// Update is called once per frame
	void Update () {
		if(Camera.main.fieldOfView <= 5)
		{
			Camera.main.fieldOfView = 5;
		}
		if(Input.GetKey(KeyCode.UpArrow))
		{
			CameraAxe.position = new Vector3(CameraAxe.position.x, CameraAxe.position.y, CameraAxe.position.z-10);
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			CameraAxe.position = new Vector3(CameraAxe.position.x, CameraAxe.position.y, CameraAxe.position.z+10);
		}
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			CameraAxe.position = new Vector3(CameraAxe.position.x+10, CameraAxe.position.y, CameraAxe.position.z);
		}
		if(Input.GetKey(KeyCode.RightArrow))
		{
			CameraAxe.position = new Vector3(CameraAxe.position.x-10, CameraAxe.position.y, CameraAxe.position.z);
		}
		if(Input.GetKey(KeyCode.Escape))
		{
			if(currentMode == 0 && editorMode == 0)
			{
				if(selectedSystem)
				{
					selectedSystem.select(false);
					selectedSystem = null;
				}
				if(selectedNebula)
				{
					selectedNebula.select(false);
					selectedNebula = null;
				}
				if(prevSelectedSystem)
				{
					prevSelectedSystem.select(false);
					prevSelectedSystem = null;
				}
				if(prevSelectedNebula)
				{
					prevSelectedNebula.select(false);
					prevSelectedNebula = null;
				}
				if(prevSelectedHyperlane)
				{
					prevSelectedHyperlane = null;
				}
			}
			else
			{
				currentMode = 0;
			}
		}
		if(Input.GetKey(KeyCode.Delete))
		{
			if(editorMode == 0)
			{
				if(selectedSystem)
				{
					Destroy(selectedSystem.gameObject);
				}
				if(selectedNebula)
				{
					Destroy(selectedNebula.gameObject);
				}
			}
		}
		if(Input.GetKeyDown(KeyCode.F1))
		{
			showTexts = !showTexts;
		}
		if(GUIUtility.hotControl == 0)
		{
			if(!scrollerRect.Contains(Input.mousePosition))
			{
				if(Input.GetAxis("Mouse ScrollWheel") < 0)
				{
					// Debug.Log(GUIUtility.hotControl);
					Camera.main.fieldOfView+=5;
				}
				if(Input.GetAxis("Mouse ScrollWheel") > 0)
				{
					Camera.main.fieldOfView-=5;
				}
			}
			if(Input.GetKeyDown(KeyCode.Mouse0) && editorMode == 0)
			{
				switch(main.currentMode)
				{
					case 3:
						Vector3 poss = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 460));
						GameObject g = (GameObject)Instantiate(system_prefab, new Vector3(poss.x, 0, poss.z), Quaternion.identity);
						g.transform.SetParent(GalaxyEditor.transform, true);
						break;
						
					case 5:
						Vector3 posn = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 460));
						GameObject g2 = (GameObject)Instantiate(nebula_prefab, new Vector3(posn.x, 0, posn.z), Quaternion.identity);
						g2.transform.SetParent(GalaxyEditor.transform, true);
						break;
				}
			}
		}
	}
	
    void OnGUI()
    {
		if(editorMode == 0 && !isGenerating)
		{
			refreshGUIGalaxy();
		}
		else if(isGenerating)
		{
			refreshGUIGenerating();
		}
    }
	public static void refreshGalaxySizeLines(int size)
	{
		float rp = size/2;
		float rn = (size/2)-size;
		lineUp.SetPosition(0,new Vector3(rn,0,rn));
		lineUp.SetPosition(1,new Vector3(rp,0,rn));
		lineDown.SetPosition(0,new Vector3(rn,0,rp));
		lineDown.SetPosition(1,new Vector3(rp,0,rp));
		lineLeft.SetPosition(0,new Vector3(rp,0,rp));
		lineLeft.SetPosition(1,new Vector3(rp,0,rn));
		lineRight.SetPosition(0,new Vector3(rn,0,rn));
		lineRight.SetPosition(1,new Vector3(rn,0,rp));
	}
	
	void save()
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("star_system");
		GameObject[] gameObjectsN = GameObject.FindGameObjectsWithTag ("nebula");
		GameObject[] gameObjectsHL = GameObject.FindGameObjectsWithTag ("hyperlane");
		int savespawns = 0;
		for(var i = 0 ; i < gameObjects.Length ; i ++)
		{
			if(gameObjects[i])
			{
				if(gameObjects[i].GetComponent<system>().isSpawn)
				{
					savespawns++;
				}
			}
		}
		string file = galaxyname.Replace(" ","_");
		if(!Directory.Exists("GeneratedMods"))
			Directory.CreateDirectory("GeneratedMods");
		if(Directory.Exists("GeneratedMods\\"+file))
			Directory.Delete("GeneratedMods\\"+file, true);
		if(!Directory.Exists("GeneratedMods\\"+file))
		{
			Directory.CreateDirectory("GeneratedMods\\"+file);
			Directory.CreateDirectory("GeneratedMods\\"+file+"\\common");
			Directory.CreateDirectory("GeneratedMods\\"+file+"\\common\\on_actions");
			Directory.CreateDirectory("GeneratedMods\\"+file+"\\common\\solar_system_initializers");
			Directory.CreateDirectory("GeneratedMods\\"+file+"\\events");
			Directory.CreateDirectory("GeneratedMods\\"+file+"\\map");
			Directory.CreateDirectory("GeneratedMods\\"+file+"\\map\\setup_scenarios");
			
			TextAsset filetoadd = (TextAsset)Resources.Load("common/on_actions/reta_events", typeof(TextAsset));
			File.WriteAllText("GeneratedMods\\"+file+"\\common\\on_actions\\reta_events.txt", filetoadd.text);
			filetoadd = (TextAsset)Resources.Load("common/solar_system_initializers/retalyx_initializers", typeof(TextAsset));
			File.WriteAllText("GeneratedMods\\"+file+"\\common\\solar_system_initializers\\retalyx_initializers.txt", filetoadd.text);
			filetoadd = (TextAsset)Resources.Load("common/solar_system_initializers/retalyx_initializers_lists", typeof(TextAsset));
			File.WriteAllText("GeneratedMods\\"+file+"\\common\\solar_system_initializers\\retalyx_initializers_lists.txt", filetoadd.text);
			filetoadd = (TextAsset)Resources.Load("events/reta_start", typeof(TextAsset));
			File.WriteAllText("GeneratedMods\\"+file+"\\events\\reta_start.txt", filetoadd.text);
		}
		string fileName = "GeneratedMods\\"+file+"\\map\\setup_scenarios\\retalyx_"+file+".txt";
		StreamWriter sr = File.CreateText(fileName);
		sr.WriteLine("#Made with Retalyx Static Galaxy Generator v"+version);
		sr.WriteLine("static_galaxy_scenario = {");
		sr.WriteLine("    name = \""+galaxyname+"\"");
		sr.WriteLine("    priority = 0");
		sr.WriteLine("    default = no");
		sr.WriteLine("    colonizable_planet_odds = 1.0");
		sr.WriteLine("    num_empires = { min = 1 max = "+(savespawns-1)+" }");
		sr.WriteLine("    num_empire_default = "+(savespawns-1));
		sr.WriteLine("    advanced_empire_default = 0");
		sr.WriteLine("    core_radius = 0");
		int customSys = 0;
		if(autogenHyperlines)
		{
			sr.WriteLine("    random_hyperlanes = yes");
		}
		else
		{
			sr.WriteLine("    random_hyperlanes = no");
		}
		
		for(var i = 0 ; i < gameObjects.Length ; i ++)
		{
			if(gameObjects[i])
			{
				sr.WriteLine("");
				sr.WriteLine("    system = {");
				if(gameObjects[i].GetComponent<system>().systemName != null && gameObjects[i].GetComponent<system>().systemName != "" && gameObjects[i].GetComponent<system>().systemName != "RandomName" && gameObjects[i].GetComponent<system>().systemName != "random")
				{
					sr.WriteLine("        name = \""+gameObjects[i].GetComponent<system>().systemName+"\"");
				}	
				sr.WriteLine("        id = \""+gameObjects[i].GetComponent<system>().systemid+"\"");
				sr.WriteLine("        position = {");
				sr.WriteLine("            x = "+gameObjects[i].transform.position.x+"");
				sr.WriteLine("            y = "+gameObjects[i].transform.position.z+"");
				sr.WriteLine("        }");
				if(gameObjects[i].GetComponent<system>().isSpawn)
				{
					if(gameObjects[i].GetComponent<system>().initializer != null && gameObjects[i].GetComponent<system>().initializer != "")
					{
						sr.WriteLine("        initializer = "+gameObjects[i].GetComponent<system>().initializer);
						customSys++;
					}
					else
					{
						sr.WriteLine("        initializer = retalyx_random_spawn");
					}
					sr.WriteLine("        spawn_weight = { base = 1 }");
				}
				else if(gameObjects[i].GetComponent<system>().initializer != null && gameObjects[i].GetComponent<system>().initializer != "")
				{
					sr.WriteLine("        initializer = "+gameObjects[i].GetComponent<system>().initializer);
					customSys++;
				}
				sr.WriteLine("    }");
				if(gameObjects[i].GetComponent<system>().prevent)
				{
					sr.WriteLine("    prevent_hyperlane = { from = \""+gameObjects[i].GetComponent<system>().systemid+"\" to = \"any\" }");
				}
			}
		}
		
		for(var i = 0 ; i < gameObjectsN.Length ; i ++)
		{
			if(gameObjectsN[i])
			{
				sr.WriteLine("");
				sr.WriteLine("    nebula = {");
				sr.WriteLine("        name = \""+gameObjectsN[i].GetComponent<nebula>().nebulaName+"\"");
				sr.WriteLine("        position = {");
				sr.WriteLine("            x = "+gameObjectsN[i].transform.position.x+"");
				sr.WriteLine("            y = "+gameObjectsN[i].transform.position.z+"");
				sr.WriteLine("        }");
				sr.WriteLine("        radius = "+gameObjectsN[i].GetComponent<nebula>().nebulaRadius+"");
				sr.WriteLine("    }");
			}
		}
		sr.WriteLine("");
		
		for(var i = 0 ; i < gameObjectsHL.Length ; i ++)
		{
			if(gameObjectsHL[i])
			{
				if(gameObjectsHL[i].GetComponent<hyperlane>().prevent)
				{
					sr.WriteLine("    prevent_hyperlane = { from = \""+gameObjectsHL[i].GetComponent<hyperlane>().link0.systemid+"\" to = \""+gameObjectsHL[i].GetComponent<hyperlane>().link1.systemid+"\" }");
				}
				else
				{
					sr.WriteLine("    add_hyperlane = { from = \""+gameObjectsHL[i].GetComponent<hyperlane>().link0.systemid+"\" to = \""+gameObjectsHL[i].GetComponent<hyperlane>().link1.systemid+"\" }");
				}
			}
		}
		sr.WriteLine("}");
		sr.Close();	
		UnityAnalytics.CustomEvent("savedGalaxy", new Dictionary<string, object>
		{
			{ "systems", gameObjects.Length },
			{ "nebulas", gameObjectsN.Length },
			{ "hyperlanes", gameObjectsHL.Length },
			{ "customSystems", customSys }
		});
		
		for(var i = 0 ; i < systemEditors.Count ; i ++)
		{
			systemEditors[i].GetComponent<systemeditor>().save();
		}
		
		fileName = "GeneratedMods\\"+file+".mod";
		sr = File.CreateText(fileName);
		sr.WriteLine("name=\""+galaxyname+"\"");
		sr.WriteLine("path=\"mod/"+file+"\"");
		sr.WriteLine("tags={");
		sr.WriteLine("	\"Galaxy Generation\"");
		sr.WriteLine("}");
		sr.WriteLine("supported_version=\""+supportedGameVersion+"\"");
		sr.Close();	
		
		
		Log("Saved galaxy in\nGeneratedMods\\"+file);
	}
	
	void generate()
	{
		Perlin pnoise = new Perlin();
		pnoise.Seed = (int)generationseed;
		pnoise.Persistence = npers;
		pnoise.Frequency = nfreq;
		UnityEngine.Random.seed = (int)generationseed;
		ResetAll();
		
		float rp = galaxysize/2;
		float rn = (galaxysize/2)-galaxysize;
		
		List<Transform> systemsP = new List<Transform>();
		for(int x=0;x < starstogen;x++)
		{
			int posX = (int)Mathf.Round(UnityEngine.Random.Range(rn, rp));
			int posY = (int)Mathf.Round(UnityEngine.Random.Range(rn, rp));
			bool cancel = false;
			if(minDist > 0)
			{
				for(int j = 0 ; j < systemsP.Count ; j++)
				{
					float d = Vector3.Distance(systemsP[j].position, new Vector3(posX,0,posY));
					if(d < minDist)
					{
						cancel = true;
					}
				}
			}
			if(!cancel)
			{
				if(pnoise.GetValue((double)posX/nscale, 0, (double)posY/nscale) > 0 || !usePerlin)
				{
					GameObject gs = (GameObject)Instantiate(system_prefab, new Vector3(posX,0,posY), Quaternion.identity);
					gs.transform.SetParent(GalaxyEditor.transform, true);
					systemsP.Add(gs.transform);
				}
			}
		}
			
		refreshGalaxySizeLines((int)galaxysize);			
		Log("Galaxy Generated");
	}
	
	void generatefrompng()
	{
		Perlin pnoise = new Perlin();
		pnoise.Seed = (int)generationseed;
		pnoise.Persistence = npers;
		pnoise.Frequency = nfreq;
		UnityEngine.Random.seed = (int)generationseed;
		ResetAll();
		
		Texture2D tex = LoadPNG("galaxy.png");
		pnggalaxyX = (int)Mathf.Round(tex.width*pngmultiplier);
		pnggalaxyY = (int)Mathf.Round(tex.height*pngmultiplier);
		List<Transform> systemsP = new List<Transform>();
		for(int x = 0; x < tex.width; x++)
		{
			for(int y = 0; y < tex.height; y++)
			{
				float reduce = UnityEngine.Random.value;
				if(pnoise.GetValue((double)x/nscale, 0, (double)y/nscale) > 0 || !usePerlin)
				{
					// if(tex.GetPixel(x,y) == new Color(0,0,0,1) && reduce > pngreducer)
					if(reduce > pngreducer && tex.GetPixel(x,y).r == tex.GetPixel(x,y).g && tex.GetPixel(x,y).r == tex.GetPixel(x,y).b && tex.GetPixel(x,y).a == 1)
					{
						float spawnchance = UnityEngine.Random.value;
						float spawnchnceMin = tex.GetPixel(x,y).r;
						int newx = x-(tex.width/2);
						int newy = y-(tex.height/2);
						float ranp = pngrandomizer/2;
						float rann = (pngrandomizer/2)-pngrandomizer;
						int offsetx = (int)Mathf.Round(UnityEngine.Random.Range(rann, ranp));
						int offsety = (int)Mathf.Round(UnityEngine.Random.Range(rann, ranp));
						newx = (int)Mathf.Round((newx*pngmultiplier)+offsetx);
						newy = (int)Mathf.Round((newy*pngmultiplier)+offsety);
						bool cancel = false;
						if(minDist > 0)
						{
							for(int j = 0 ; j < systemsP.Count ; j++)
							{
								float d = Vector3.Distance(systemsP[j].position, new Vector3(newx,0,newy));
								if(d < minDist)
								{
									cancel = true;
								}
							}
						}
						if(spawnchance > spawnchnceMin && !cancel)
						{
							GameObject g = (GameObject)Instantiate(system_prefab, new Vector3(newx,0,newy), Quaternion.identity);
							g.transform.SetParent(GalaxyEditor.transform, true);
							systemsP.Add(g.transform);
						}
					}
				}
			}
		}
		systemsP.Clear();
		float rpx = (tex.width/2)*pngmultiplier;
		float rnx = ((tex.width/2)-tex.width)*pngmultiplier;
		float rpy = (tex.height/2)*pngmultiplier;
		float rny = ((tex.height/2)-tex.height)*pngmultiplier;
		lineUp.SetPosition(0,new Vector3(rnx,0,rny));
		lineUp.SetPosition(1,new Vector3(rpx,0,rny));
		lineDown.SetPosition(0,new Vector3(rnx,0,rpy));
		lineDown.SetPosition(1,new Vector3(rpx,0,rpy));
		lineLeft.SetPosition(0,new Vector3(rpx,0,rpy));
		lineLeft.SetPosition(1,new Vector3(rpx,0,rny));
		lineRight.SetPosition(0,new Vector3(rnx,0,rny));
		lineRight.SetPosition(1,new Vector3(rnx,0,rpy));
		Log("Galaxy Generated from galaxy.png");
	}
	
	public static Texture2D LoadPNG(string filePath)
	{
		Texture2D img = null;
		byte[] fileData;

		if(File.Exists(filePath))
		{
			fileData = File.ReadAllBytes(filePath);
			img = new Texture2D(2, 2);
			img.LoadImage(fileData);
		}
		return img;
	}
	
	public static void Log(string m)
    {
		string t = SpliceText(m, 28);
        message += t + "\n";
    }
	
	public static void ClearLog()
    {
        message = "";
    }
	
	public static string SpliceText(string text, int lineLength) 
	{
		return Regex.Replace(text, "(.{" + lineLength + "})", "$1" + Environment.NewLine);
	}

	void donate()
	{
		Application.OpenURL("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=retalyx%40retalyx%2efr&lc=FR&no_note=0&cn=Ajouter%20des%20instructions%20particuli%c3%a8res%20pour%20le%20vendeur%20%3a&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
	}
	
	public static void ResetSystems()
	{
		foreach(system go in systems)
		{
			if(go)
			{
				Destroy(go.gameObject);
			}
		}
		systemid = 0;
		spawns = 0;
		systems.Clear();
		ResetSystemEditors();
	}
	
	public static void ResetSystemEditors()
	{
		foreach(GameObject go in systemEditors)
		{
			if(go)
			{
				Destroy(go);
			}
		}
		systemEditors.Clear();
		planetid = 0;
	}
	
	public static void ResetNebulas()
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("nebula");
		foreach(GameObject go in gameObjects)
		{
			if(go)
			{
				Destroy(go);
			}
		}
	}
	
	public static void ResetHyperlanes()
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("hyperlane");
		foreach(GameObject go in gameObjects)
		{
			if(go)
			{
				Destroy(go);
			}
		}
	}
	
	public static void ResetAll()
	{
		ResetSystems();
		ResetNebulas();
		ResetHyperlanes();
	}

	// void import()
	// {
		// List<List<List<String>>> fileLines;
		// var sr = File.OpenText("galaxy.txt");
		// fileLines = sr.ReadToEnd().Split('{').Select(s=>s.Split('\n').Select(t=>t.Split('=').ToList()).ToList()).ToList();
		// sr.Close();
		// foreach(List<List<String>> line in fileLines)
		// {
			// Debug.Log(line[0][0]);
		// }
	// }
	
	private bool IsPointerOverUIObject() {
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
	
	IEnumerator generateHyperlanes()
	{
		isGenerating = true;
		float progressPercent = 0f;
		float timeLeft = 0f;
		List<float> times = new List<float>();
		var temp = Time.realtimeSinceStartup;
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("star_system");
		int cnt = 0;
		int lcnt = 0;
		
		foreach(GameObject sy in gameObjects)
		{
			var tempL = Time.realtimeSinceStartup;
			if(sy)
			{
				float[] distances = new float[gameObjects.Length];
				int[] indexes = new int[gameObjects.Length];
				int arrayind = 0;
				int j = 0;
				foreach(GameObject go in gameObjects)
				{
					float dist = Vector3.Distance(sy.transform.position, go.transform.position);
					if(dist <= genhlradius && dist > 0)
					{
						distances[arrayind] = dist;
						indexes[arrayind] = j;
						arrayind++;
					}
					j++;
				}
				foreach(float distance in distances)
				{
					if(distance <= 0)
					{
						distances[Array.IndexOf(distances, distance)] = genhlradius+1;
					}
				}
				Array.Sort(distances, indexes);
				Array.Resize(ref distances, genhlmaxlines+1);
				Array.Resize(ref indexes, genhlmaxlines+1);
				for(int i = 0; i < distances.Length; i++)
				{
					float mindDist  = Mathf.Min(distances);
					int ind = indexes[Array.IndexOf(distances, mindDist)];
					system sys0 = sy.GetComponent<system>();
					system sys1 = gameObjects[ind].GetComponent<system>();
					distances[Array.IndexOf(distances, mindDist)] = genhlradius+1;
					bool ignorelane = false;
					GameObject[] hlt = GameObject.FindGameObjectsWithTag ("hyperlane");
					foreach(GameObject ht in hlt)
					{
						if(ht.name == "hl_"+sys0.systemid+"_to_"+sys1.systemid || ht.name == "hl_"+sys1.systemid+"_to_"+sys0.systemid)
						{
							ignorelane = true;
							// Debug.Log("Ignoring:" + ht.name);
							break;
						}
					}
					
					if(!ignorelane)
					{
						float checkdist = Vector3.Distance(sy.transform.position, gameObjects[ind].transform.position);
						if(checkdist <= genhlradius && sy != gameObjects[ind] && sys0.connectedLines < genhlmaxlines && sys1.connectedLines < genhlmaxlines && checkdist > 0 && !sys0.prevent && !sys1.prevent)
						{
							GameObject line = (GameObject)Instantiate(hyperlane_prefab, new Vector3(0, 0, 0), Quaternion.identity);
							line.transform.SetParent(GalaxyEditor.transform, true);
							hyperlane hl = line.GetComponent<hyperlane>();
							hl.link0 = sys0;
							hl.link1 = sys1;
							hl.transform.position = sys0.transform.position;
							hl.SetName();
							sys0.connectedLines++;
							sys1.connectedLines++;
							cnt++;
						}
					}
				}
			}
			lcnt++;
			//Stats
			if(lcnt % 10 == 0)
			{
				float thislTime = float.Parse((Time.realtimeSinceStartup - tempL).ToString("f3"));
				times.Add(thislTime);
				progressPercent = (float)lcnt / (float)gameObjects.Length * 100;
				timeLeft = ((float)gameObjects.Length - (float)lcnt) * times.Average();
				TimeSpan t = TimeSpan.FromSeconds( timeLeft );
				TimeSpan tt = TimeSpan.FromSeconds( thislTime );
				string timeLeftString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", 
					t.Hours, 
					t.Minutes, 
					t.Seconds);
				string thislTimeString = string.Format("{0:D2}m:{1:D2}s:{2:D3}ms", 
					tt.Minutes, 
					tt.Seconds, 
					tt.Milliseconds);
				generationStatusString = "Generating: " + lcnt + "/" + gameObjects.Length + "\nHyperlanes: " + cnt + "\nLast gen duration: " + thislTimeString +"\n\nProgress: " + progressPercent.ToString("f2") + "%" + "\nEstimated Time Left: " + timeLeftString;
				yield return new WaitForSeconds(0.001f);
			}
		}
		string tText = (Time.realtimeSinceStartup - temp).ToString("f2");
		Log("Generated "+cnt+" hyperlanes in\n"+tText+"s");
		UnityAnalytics.CustomEvent("genHyperlane", new Dictionary<string, object>
		{
			{ "radius", genhlradius },
			{ "maxLines", genhlmaxlines },
			{ "finisgHyperlane", cnt },
			{ "finishDuration", tText }
		});
		isGenerating = false;
	}
	
	private void refreshGUIGalaxy()
	{
		//Left Panel
		GUILayout.BeginArea(new Rect(10, 10, 200, 1000));
			
		GUILayout.Label("Galaxy Generator by Retalyx V"+version);
		if (GUILayout.Button("Donate"))
		{
			donate();
		}
		
		GUILayout.Label("Galaxy Name:");
		galaxyname = GUILayout.TextField(galaxyname, 32);
			
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Seed:");
		generationseed = int.Parse(GUILayout.TextField(generationseed+"", 7));
		GUILayout.EndHorizontal();
		generationseed = (int)Mathf.Round(GUILayout.HorizontalSlider(generationseed, 0.0F, 1000000.0F));
		autogenHyperlines = GUILayout.Toggle(autogenHyperlines, "Auto Generate Hyperlanes");
		GUILayout.Label("Minimal System Distance: "+minDist);
		minDist = (int)Mathf.Round(GUILayout.HorizontalSlider(minDist, 0.0F, 50.0F));
		loadfromfile = GUILayout.Toggle(loadfromfile, "Import from file");
		if(!loadfromfile)
		{
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Galaxy Size:");
			galaxysize = float.Parse(GUILayout.TextField(galaxysize+"", 4));
			GUILayout.EndHorizontal();
			galaxysize = (int)Mathf.Round(GUILayout.HorizontalSlider(galaxysize, 0.0F, 1000.0F));
			if(galaxysize > 1000)
			{
				GUI.color = Color.red;
				GUILayout.Label("MAX INGAME SUPPORTED SIZE IS 1000");
				GUI.color = Color.white;
			}
			GUILayout.Label("Real System Count: "+galaxysystemcount);
			GUILayout.Label("Nebula Count: "+galaxynebulacount);
			
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("System Count:");
			starstogen = float.Parse(GUILayout.TextField(starstogen+"", 5));
			GUILayout.EndHorizontal();
			starstogen = (int)Mathf.Round(GUILayout.HorizontalSlider(starstogen, 0.0F, 50000.0F));
		
			usePerlin = GUILayout.Toggle(usePerlin, "Perlin Noise");
			
			if(usePerlin)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Noise Scale:");
				nscale = float.Parse(GUILayout.TextField(nscale+"", 7));
				GUILayout.EndHorizontal();
				nscale = GUILayout.HorizontalSlider(nscale, 0.0F, 1000.0F);
				
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Noise Persistence:");
				npers = float.Parse(GUILayout.TextField(npers+"", 4));
				GUILayout.EndHorizontal();
				npers = GUILayout.HorizontalSlider(npers, 0.0F, 2.0F);
				
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Noise Frequency:");
				nfreq = float.Parse(GUILayout.TextField(nfreq+"", 4));
				GUILayout.EndHorizontal();
				nfreq = GUILayout.HorizontalSlider(nfreq, 0.0F, 5.0F);
			}
			
			if (GUILayout.Button("Generate"))
			{
				generate();
			}
		}
		else
		{
			GUILayout.Label("Galaxy Size: "+Mathf.Max(pnggalaxyX,pnggalaxyY));
			GUILayout.Label("System Count: "+galaxysystemcount);
			GUILayout.Label("Nebula Count: "+galaxynebulacount);
			if(Mathf.Max(pnggalaxyX,pnggalaxyY) > 1000)
			{
				GUI.color = Color.red;
				GUILayout.Label("MAX INGAME SUPPORTED SIZE IS 1000");
				GUI.color = Color.white;
			}
		
			usePerlin = GUILayout.Toggle(usePerlin, "Perlin Noise");
			
			if(usePerlin)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Noise Scale:");
				nscale = float.Parse(GUILayout.TextField(nscale+"", 7));
				GUILayout.EndHorizontal();
				nscale = GUILayout.HorizontalSlider(nscale, 0.0F, 1000.0F);
				
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Noise Persistence:");
				npers = float.Parse(GUILayout.TextField(npers+"", 4));
				GUILayout.EndHorizontal();
				npers = GUILayout.HorizontalSlider(npers, 0.0F, 2.0F);
				
				GUILayout.BeginHorizontal("box");
				GUILayout.Label("Noise Frequency:");
				nfreq = float.Parse(GUILayout.TextField(nfreq+"", 4));
				GUILayout.EndHorizontal();
				nfreq = GUILayout.HorizontalSlider(nfreq, 0.0F, 5.0F);
			}
			
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Scale:");
			pngmultiplier = float.Parse(GUILayout.TextField(pngmultiplier+"", 5));
			GUILayout.EndHorizontal();
			pngmultiplier = (int)Mathf.Round(GUILayout.HorizontalSlider(pngmultiplier, 0.0F, 25.0F));
			
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Randomizer:");
			pngrandomizer = float.Parse(GUILayout.TextField(pngrandomizer+"", 5));
			GUILayout.EndHorizontal();
			pngrandomizer = (int)Mathf.Round(GUILayout.HorizontalSlider(pngrandomizer, 0.0F, 25.0F));
			
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Reducer:");
			pngreducer = float.Parse(GUILayout.TextField(pngreducer+"", 4));
			GUILayout.EndHorizontal();
			pngreducer = GUILayout.HorizontalSlider(pngreducer, 0.0F, 1.0F);
			
			if (GUILayout.Button("Generate From PNG"))
			{
				generatefrompng();
			}
		}
		if(spawns >= 2)
		{
			GUI.color = Color.green;
			if (GUILayout.Button("Save Mod"))
			{
				save();
			}
			GUI.color = Color.white;
		}
		else
		{
			GUI.color = Color.red;
			if (GUILayout.Button("Save Mod"))
			{
				Log("You need at least 2 spawns\nto save your galaxy");
			}
			GUI.color = Color.white;
		}
		if(lv != null && lvdownload != null)
		{
			if(float.Parse(lv) > float.Parse(version))
			{
				GUILayout.Label("New Version available!");
				if(GUILayout.Button("Download V"+lv))
				{
					Application.OpenURL(lvdownload);
					
					UnityAnalytics.CustomEvent("downloadUpdate", new Dictionary<string, object>
					{
						{ "clickedUpdateButton", true },
						{ "latestVersion", lv }
					});
				}
			}
		}
		// GUILayout.Label("Warning: Experimental!");
		if(GUILayout.Button("Save/Load Project"))
		{
			editorMode = -1;
			projectManager.refreshProjectList();
		}
		
		if(message != null && message != "")
		{
			if(GUILayout.Button("Clear Logs"))
			{
				ClearLog();
			}
		}
		GUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(200), GUILayout.Height(500));
		GUILayout.Box(message);
		GUILayout.EndScrollView();
        GUILayout.EndArea();
		
		//Right Panel
		string modeText = "No Mode";
		GUILayout.BeginArea(new Rect(Screen.width-210, 10, 200, 1000));
		switch(currentMode)
		{
			case 1:
				modeText = "Spawn";
				break;
				
			case 2:
				modeText = "Move";
				break;
				
			case 3:
				modeText = "Create System";
				break;
				
			case 4:
				modeText = "Delete System";
				break;
				
			case 5:
				modeText = "Create Nebula";
				break;
				
			case 6:
				modeText = "Delete Nebula";
				break;
				
			case 7:
				modeText = "Hyperlane Tool";
				break;
				
			default:
				modeText = "No Mode";
				break;
		}
		GUILayout.Label("Current Mode: " + modeText);
			
		if (GUILayout.Button("Spawn"))
		{
			if(currentMode == 1)
			{
				currentMode = 0;
			}
			else
			{
				currentMode = 1;
			}
		}
		if(currentMode == 1)
		{
			if (GUILayout.Button("Generate "+genspawn+" Spawn"))
			{
				GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("star_system");
				int maxSpawn = 0;
				for(var i = 0 ; i < gameObjects.Length ; i ++)
				{
					if(gameObjects[i])
					{
						if(!gameObjects[i].GetComponent<system>().isSpawn)
						{
							maxSpawn++;
						}
					}
				}
				if(genspawn > maxSpawn)
				{
					genspawn = maxSpawn;
				}
				for(var i = 0 ; i < genspawn ; i ++)
				{
					int rn = UnityEngine.Random.Range(0,gameObjects.Length);
					if(gameObjects[rn].GetComponent<system>().isSpawn)
					{
						i--;
					}
					else
					{
						gameObjects[rn].GetComponent<system>().setSpawn(true);
						// spawns++;
					}
				}
				Log("Generated "+genspawn+" new spawns");
			}
			genspawn = int.Parse(GUILayout.TextField(genspawn+"", 3));
		}
		if (GUILayout.Button("Move"))
		{
			if(currentMode == 2)
			{
				currentMode = 0;
			}
			else
			{
				currentMode = 2;
			}
		}
			
		GUILayout.BeginHorizontal("box");
		if (GUILayout.Button("Create System"))
		{
			if(currentMode == 3)
			{
				currentMode = 0;
			}
			else
			{
				currentMode = 3;
			}
		}
		if (GUILayout.Button("Delete System"))
		{
			if(currentMode == 4)
			{
				currentMode = 0;
			}
			else
			{
				currentMode = 4;
			}
		}
		GUILayout.EndHorizontal();
			
		GUILayout.BeginHorizontal("box");
		if (GUILayout.Button("Create Nebula"))
		{
			if(currentMode == 5)
			{
				currentMode = 0;
			}
			else
			{
				currentMode = 5;
			}
		}
		if (GUILayout.Button("Delete Nebula"))
		{
			if(currentMode == 6)
			{
				currentMode = 0;
			}
			else
			{
				currentMode = 6;
			}
		}
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Hyperlane"))
		{
			if(currentMode == 7)
			{
				currentMode = 0;
				prevSelectedHyperlane = null;
				
			}
			else
			{
				currentMode = 7;
			}
		}
		if(currentMode == 7)
		{
			// hlpreventmode = GUILayout.Toggle(hlpreventmode, "Prevent mode");
			if (GUILayout.Button("Radius Generate Hyperlane"))
			{
				StartCoroutine(generateHyperlanes());
			}
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Generation Radius:");
			genhlradius = int.Parse(GUILayout.TextField(genhlradius+"", 6));
			GUILayout.EndHorizontal();
			genhlradius = (int)Mathf.Round(GUILayout.HorizontalSlider(genhlradius, 0.0F, 100.0F));
			
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Max per System:");
			genhlmaxlines = int.Parse(GUILayout.TextField(genhlmaxlines+"", 2));
			GUILayout.EndHorizontal();
			genhlmaxlines = (int)Mathf.Round(GUILayout.HorizontalSlider(genhlmaxlines, 0.0F, 10.0F));
			
			if (GUILayout.Button("Clear Hyperlanes"))
			{
				GameObject[] la = GameObject.FindGameObjectsWithTag ("hyperlane");
				for(var w = 0 ; w < la.Length ; w ++)
				{
					if(la[w])
					{
						la[w].GetComponent<hyperlane>().kill();
					}
				}
			}
		}
		if(selectedSystem)
		{
			GUILayout.Label("Selected System Infos");
			if(selectedSystem.systemName != "RandomName" && selectedSystem.systemName != "random" && selectedSystem.systemName != "" && selectedSystem.systemName != null)
			{
				GUILayout.Label("Name: " + selectedSystem.systemName);
			}
			GUILayout.Label("Position");
			GUILayout.Label("X: "+selectedSystem.transform.position.x);
			GUILayout.Label("Y: "+selectedSystem.transform.position.z);
			GUILayout.Label("ID: "+selectedSystem.systemid);
			GUILayout.Label("Hyperlanes: "+selectedSystem.connectedLines);
			if(!selectedSystem.prevent)
			{
				if(GUILayout.Button("Prevent Hyperlanes to any"))
				{
					selectedSystem.prevent = true;
					GameObject[] gameObjectsHL = GameObject.FindGameObjectsWithTag ("hyperlane");
					for(var i = 0 ; i < gameObjectsHL.Length ; i ++)
					{
						if(gameObjectsHL[i])
						{
							if(gameObjectsHL[i].GetComponent<hyperlane>().link0 == selectedSystem || gameObjectsHL[i].GetComponent<hyperlane>().link1 == selectedSystem)
							{
								gameObjectsHL[i].GetComponent<hyperlane>().kill();
							}
						}
					}
				}
			}
			else
			{
				if(GUILayout.Button("Allow Hyperlanes to any"))
				{
					selectedSystem.prevent = false;
				}
			}
			if(GUILayout.Button("Delete connected hyperlanes"))
			{
				GameObject[] gameObjectsHL = GameObject.FindGameObjectsWithTag ("hyperlane");
				for(var i = 0 ; i < gameObjectsHL.Length ; i ++)
				{
					if(gameObjectsHL[i])
					{
						if(gameObjectsHL[i].GetComponent<hyperlane>().link0 == selectedSystem || gameObjectsHL[i].GetComponent<hyperlane>().link1 == selectedSystem)
						{
							gameObjectsHL[i].GetComponent<hyperlane>().kill();
						}
					}
				}
			}
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Initializer:");
			selectedSystem.initializer = GUILayout.TextField(selectedSystem.initializer);
			GUILayout.EndHorizontal();
			if(GUILayout.Button("Edit in System Editor"))
			{
				editorMode = 1;
				GalaxyEditor.SetActive(false);
				bool fnd = false;
				foreach(GameObject se in systemEditors)
				{
					if(se.GetComponent<systemeditor>().thisSystem == selectedSystem)
					{
						se.SetActive(true);
						fnd = true;
						break;
					}
				}
				if(!fnd)
				{
					GameObject se = (GameObject)Instantiate(systemeditor_prefab, new Vector3(0, 0, 0), Quaternion.identity);
					se.GetComponent<systemeditor>().SetForSys(selectedSystem);
					systemEditors.Add(se);
				}
			}
			if(selectedSystem.isSpawn)
			{
				if(GUILayout.Button("Disable Spawn"))
				{
					selectedSystem.setSpawn(false);
					// spawns--;
				}
			}
			else if(!selectedSystem.isSpawn)
			{
				if(GUILayout.Button("Enable Spawn"))
				{
					selectedSystem.setSpawn(true);
					// spawns++;
				}
			}
		}
		if(selectedNebula)
		{
			GUILayout.Label("Selected Nebula Infos");
			GUILayout.Label("Position");
			GUILayout.Label("X: "+selectedNebula.transform.position.x);
			GUILayout.Label("Y: "+selectedNebula.transform.position.z);
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Name:");
			selectedNebula.nebulaName = GUILayout.TextField(selectedNebula.nebulaName);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Radius:");
			selectedNebula.nebulaRadius = int.Parse(GUILayout.TextField(selectedNebula.nebulaRadius+"", 3));
			GUILayout.EndHorizontal();
		}
        GUILayout.EndArea();
	}
	
	private void refreshGUIGenerating()
	{
		GUILayout.BeginArea(new Rect(10, 10, 300, 1000));
		GUILayout.Label(generationStatusString);
        GUILayout.EndArea();
	}
	
	private void loadPresets()
	{
		string fileName = "presets.json";
		
		try
		{
			if(!File.Exists(fileName))
			{
				TextAsset filetoadd = (TextAsset)Resources.Load("presets", typeof(TextAsset));
				File.WriteAllText(fileName, filetoadd.text);
			}
		}
		catch
		{
			Log("Error while creating presets file");
		}
		
		var sr = File.OpenText(fileName);
		string filetext = sr.ReadToEnd();
		var N = JSON.Parse(filetext);
		
		try
		{
			for(int x = 0; x < N["planet_class"]["name"].Count; x++)
			{	
				planetClassPresetsN.Add(N["planet_class"]["name"][x].Value);
				planetClassPresets.Add(N["planet_class"]["value"][x].Value);
			}
			for(int x = 0; x < N["star_class"]["name"].Count; x++)
			{	
				starClassPresetsN.Add(N["star_class"]["name"][x].Value);
				starClassPresets.Add(N["star_class"]["value"][x].Value);
			}
			for(int x = 0; x < N["planet_modifier"]["name"].Count; x++)
			{	
				planetModifierPresetsN.Add(N["planet_modifier"]["name"][x].Value);
				planetModifierPresets.Add(N["planet_modifier"]["value"][x].Value);
			}
		}
		catch
		{
			Log("Error while loading presets file");
		}
	}
	
	private IEnumerator CheckVersion()
	{
		WWW w = new WWW("http://retalyx.fr/rsgg/latestversion.txt");
		yield return w;
		if (w.error != null)
		{
			Debug.Log("Error .. " +w.error);
		}
		else
		{
			Debug.Log("Found ... ==>" +w.text +"<==");
			string val = w.text;
			string[] v = val.Split('|');
			lv = v[0];
			lvdownload = v[1];
			
			if(lv != null && lvdownload != null)
			{
				if(float.Parse(lv) > float.Parse(version))
				{
					UnityAnalytics.CustomEvent("update", new Dictionary<string, object>
					{
						{ "needUpdate", true },
						{ "currentVersion", version }
					});
				}
				else
				{
					UnityAnalytics.CustomEvent("update", new Dictionary<string, object>
					{
						{ "needUpdate", false },
						{ "currentVersion", version }
					});
				}
			}
		}
	}
}
