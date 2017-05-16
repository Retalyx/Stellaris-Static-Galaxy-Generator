using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System;
using SimpleJSON;

public class ProjectManager : MonoBehaviour {
	public GameObject system_prefab;
	public GameObject nebula_prefab;
	public GameObject hyperlane_prefab;
	public GameObject systemeditor_prefab;
	public GameObject planet_prefab;
	List<string> projectList = new List<string>();
	public bool loadHyperlanes = true;
	
	void Start()
	{
		refreshProjectList();
	}
	
    void OnGUI()
    {
		if(main.editorMode == -1 && !main.isGenerating)
		{
			refreshGUI();
		}
    }
	
	public void LoadProject(string name)
	{
		StartCoroutine(Load(name));
	}
	
	IEnumerator Load(string name)
	{
		main.isGenerating = true;
		main.generationStatusString = "Loading Project\nPlease wait...\n\nLoading: Project Settings";
		string fileName = name+".rsgg";
		main.ResetAll();
		
		var sr = File.OpenText(fileName);
		string filetext = sr.ReadToEnd();
		var N = JSON.Parse(filetext);
		
		try
		{
			if(N["galaxy"]["version"].Value != main.version)
			{
				main.Log("Warning: Project version is "+N["galaxy"]["version"].Value);
				main.Log("Your RSGG version is "+main.version);
			}
			main.galaxyname = N["galaxy"]["name"];
			main.autogenHyperlines = bool.Parse(N["galaxy"]["random_hyperlanes"]);
			main.galaxysize = int.Parse(N["galaxy"]["size"].Value);
			main.genhlradius = int.Parse(N["galaxy"]["hl_radius"].Value);
			main.genhlmaxlines = int.Parse(N["galaxy"]["hl_maxPerSys"].Value);
			main.generationseed = int.Parse(N["galaxy"]["seed"].Value);
			main.starstogen = float.Parse(N["galaxy"]["stars"].Value);
			main.nscale = float.Parse(N["galaxy"]["pnoise_scale"].Value);
			main.nfreq = float.Parse(N["galaxy"]["pnoise_freq"].Value);
			main.npers = float.Parse(N["galaxy"]["pnoise_pers"].Value);
			main.pngmultiplier = float.Parse(N["galaxy"]["png_multiplier"].Value);
			main.pngrandomizer = float.Parse(N["galaxy"]["png_randomizer"].Value);
			main.pngreducer = float.Parse(N["galaxy"]["png_reducer"].Value);
			main.usePerlin = bool.Parse(N["galaxy"]["pnoise"]);
			main.refreshGalaxySizeLines(int.Parse(N["galaxy"]["size"].Value));
			if(float.Parse(N["galaxy"]["version"].Value) >= 0.9f)
				main.minDist = int.Parse(N["galaxy"]["minimalSystemDistance"].Value);
			else
				main.minDist = 12;
			// int[] d = new int[N["galaxy"]["systems"].Count*2];
			// for(int x = 0; x < N["galaxy"]["systems"].Count; x++)
			// {
				// d[x] = int.Parse(N["galaxy"]["systems"][x]["position"]["x"].Value);
				// d[N["galaxy"]["systems"].Count+x] = int.Parse(N["galaxy"]["systems"][x]["position"]["y"].Value);
			// }
			// main.galaxysize = Mathf.Max(d);
		}
		catch
		{
			// yield return new WaitForSeconds(0.001f);
			main.isGenerating = false;
			main.Log("Error while loading project");
		}
		
		main.generationStatusString = "Loading Project\nPlease wait...\n\nLoading: Galaxy Systems";
		yield return new WaitForSeconds(0.001f);
		
		List<system> TsysList = new List<system>();
		try
		{	
			// Systems
			for(int x = 0; x < N["galaxy"]["systems"].Count; x++)
			{	
				GameObject g = (GameObject)Instantiate(system_prefab, new Vector3(float.Parse(N["galaxy"]["systems"][x]["position"]["x"]), 0, float.Parse(N["galaxy"]["systems"][x]["position"]["y"])), Quaternion.identity);
				g.transform.SetParent(main.GalaxyEditor.transform, true);
				system sys = g.GetComponent<system>();
				if(N["galaxy"]["systems"][x]["name"].Value != "null")
					sys.systemName = N["galaxy"]["systems"][x]["name"].Value;
				if(N["galaxy"]["systems"][x]["initializer"].Value != "null")
					sys.initializer = N["galaxy"]["systems"][x]["initializer"].Value;
				sys.prevent = bool.Parse(N["galaxy"]["systems"][x]["prevent_hyperlane"].Value);
				sys.systemid = int.Parse(N["galaxy"]["systems"][x]["id"].Value);
				if(bool.Parse(N["galaxy"]["systems"][x]["spawn"].Value))
					sys.setSpawn(true);
				int xx = x;
				while(TsysList.Count <= sys.systemid)
				{
					TsysList.Add(sys);
					xx++;
				}
				// TsysList.Insert(sys.systemid, sys);
			}
		}
		catch
		{
			main.isGenerating = false;
			main.Log("Error while loading project");
		}
		
		main.generationStatusString = "Loading Project\nPlease wait...\n\nLoading: Galaxy Nebulas";
		yield return new WaitForSeconds(0.001f);
			
		try
		{
			//Nebulas
			for(int x = 0; x < N["galaxy"]["nebulas"].Count; x++)
			{	
				GameObject g = (GameObject)Instantiate(nebula_prefab, new Vector3(float.Parse(N["galaxy"]["nebulas"][x]["position"]["x"]), 0, float.Parse(N["galaxy"]["nebulas"][x]["position"]["y"])), Quaternion.identity);
				g.transform.SetParent(main.GalaxyEditor.transform, true);
				nebula neb = g.GetComponent<nebula>();
				if(N["galaxy"]["nebulas"][x]["name"].Value != "null")
					neb.nebulaName = N["galaxy"]["nebulas"][x]["name"].Value;
				neb.nebulaRadius = int.Parse(N["galaxy"]["nebulas"][x]["radius"].Value);
			}
		}
		catch
		{
			// yield return new WaitForSeconds(0.001f);
			main.isGenerating = false;
			main.Log("Error while loading project");
		}
			
		if(loadHyperlanes)
		{
			main.generationStatusString = "Loading Project\nPlease wait...\n\nLoading: Galaxy Hyperlanes";
			yield return new WaitForSeconds(0.001f);
			try
			{
				//Hyperlanes
				for(int x = 0; x < N["galaxy"]["hyperlanes"].Count; x++)
				{	
					GameObject g = (GameObject)Instantiate(hyperlane_prefab, new Vector3(0,0,0), Quaternion.identity);
					g.transform.SetParent(main.GalaxyEditor.transform, true);
					hyperlane hl = g.GetComponent<hyperlane>();
					
					hl.link0 = TsysList[int.Parse(N["galaxy"]["hyperlanes"][x]["from"].Value)];
					hl.link1 = TsysList[int.Parse(N["galaxy"]["hyperlanes"][x]["to"].Value)];
					if(N["galaxy"]["hyperlanes"][x]["mode"].Value == "add")
						hl.prevent = false;
					else if(N["galaxy"]["hyperlanes"][x]["mode"].Value == "prevent")
						hl.prevent = true;
				}
			}
			catch
			{
				// yield return new WaitForSeconds(0.001f);
				main.isGenerating = false;
				main.Log("Error while loading project");
			}
		}
		main.generationStatusString = "Loading Project\nPlease wait...\n\nLoading: Custom Systems";
		yield return new WaitForSeconds(0.001f);
			
		try
		{
			//Custom Systems
			for(int x = 0; x < N["customSystems"].Count; x++)
			{	
				GameObject g = (GameObject)Instantiate(systemeditor_prefab, new Vector3(0,0,0), Quaternion.identity);
				systemeditor se = g.GetComponent<systemeditor>();
				main.systemEditors.Add(g);
				system s = TsysList[int.Parse(N["customSystems"][x]["systemID"].Value)];
				
				se.SetForSys(s);
				se.systemName = N["customSystems"][x]["name"];
				se.starType = N["customSystems"][x]["starType"];
				se.centerStarSize = int.Parse(N["customSystems"][x]["starSize"].Value);
				se.centerStarDisabled = bool.Parse(N["customSystems"][x]["disableCenterStar"].Value);
				se.asteroidBelt = bool.Parse(N["customSystems"][x]["asteroidBelt"].Value);
				se.asteroidBeltRadius = int.Parse(N["customSystems"][x]["asteroidBeltRadius"].Value);
				for(int y = 0; y < N["customSystems"][x]["planets"].Count; y++)
				{	
					GameObject planetGO = (GameObject)Instantiate(planet_prefab, new Vector3(float.Parse(N["customSystems"][x]["planets"][y]["position"]["x"]),0,float.Parse(N["customSystems"][x]["planets"][y]["position"]["y"])), Quaternion.identity);
					planetGO.transform.SetParent(g.transform, true);
					planet p = planetGO.GetComponent<planet>();
					planetGO.name = "planet_"+se.thisSystem.systemid;
					p.ownerSystem = se.thisSystem.systemid;
					if(N["customSystems"][x]["planets"][y]["name"].Value != "null")
						p.planetName = N["customSystems"][x]["planets"][y]["name"].Value;
					p.planetClass = N["customSystems"][x]["planets"][y]["class"].Value;
					p.planetSize = int.Parse(N["customSystems"][x]["planets"][y]["size"].Value);
					p.orbitDistance = int.Parse(N["customSystems"][x]["planets"][y]["orbitDistance"].Value);
					p.orbitAngle = int.Parse(N["customSystems"][x]["planets"][y]["orbitAngle"].Value);
					p.isHome = bool.Parse(N["customSystems"][x]["planets"][y]["home"].Value);
					if(p.isHome)
					{
						p.planetSize = 16;
						p.planetClass = "ideal_planet_class";
					}
						
					if(float.Parse(N["galaxy"]["version"].Value) >= 0.8f)
						p.hasRing = bool.Parse(N["customSystems"][x]["planets"][y]["ringed"].Value);
					else
						p.hasRing = false;
						
					if(float.Parse(N["galaxy"]["version"].Value) >= 0.9f)
					{
						p.noAnomaly = bool.Parse(N["customSystems"][x]["planets"][y]["no_anomaly"].Value);
						p.noResources = bool.Parse(N["customSystems"][x]["planets"][y]["no_resources"].Value);
						p.planetModifier = N["customSystems"][x]["planets"][y]["modifier"].Value;
					}
					
					g.GetComponent<systemeditor>().planets.Add(planetGO);
				
					for(int z = 0; z < N["customSystems"][x]["planets"][y]["moons"].Count; z++)
					{	
						GameObject planetmGO = (GameObject)Instantiate(planet_prefab, new Vector3(float.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["position"]["x"]),0,float.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["position"]["y"])), Quaternion.identity);
						planetmGO.transform.SetParent(g.transform, true);
						planet pm = planetmGO.GetComponent<planet>();
						planetmGO.name = "moon_"+se.thisSystem.systemid;
						pm.ownerSystem = se.thisSystem.systemid;
						pm.ownerPlanet = p;
						if(N["customSystems"][x]["planets"][y]["moons"][z]["name"].Value != "null")
							pm.planetName = N["customSystems"][x]["planets"][y]["moons"][z]["name"].Value;
						pm.planetClass = N["customSystems"][x]["planets"][y]["moons"][z]["class"].Value;
						pm.planetSize = int.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["size"].Value);
						pm.orbitDistance = int.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["orbitDistance"].Value);
						pm.orbitAngle = int.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["orbitAngle"].Value);
						pm.isHome = bool.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["home"].Value);
						if(pm.isHome)
						{
							pm.planetSize = 16;
							pm.planetClass = "ideal_planet_class";
						}
							
						if(float.Parse(N["galaxy"]["version"].Value) >= 0.8f)
							pm.hasRing = bool.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["ringed"].Value);
						else
							pm.hasRing = false;
						
						if(float.Parse(N["galaxy"]["version"].Value) >= 0.9f)
						{
							pm.noAnomaly = bool.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["no_anomaly"].Value);
							pm.noResources = bool.Parse(N["customSystems"][x]["planets"][y]["moons"][z]["no_resources"].Value);
							pm.planetModifier = N["customSystems"][x]["planets"][y]["moons"][z]["modifier"].Value;
						}
						g.GetComponent<systemeditor>().planets.Add(planetmGO);
					}
				}
				g.SetActive(false);
			}
		}
		catch
		{
			// yield return new WaitForSeconds(0.001f);
			main.isGenerating = false;
			main.Log("Error while loading project");
		}
		main.isGenerating = false;
		main.Log("Project Loaded");
	}
	
	public void SaveProject(string name)
	{
		try
		{
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("star_system");
			GameObject[] gameObjectsN = GameObject.FindGameObjectsWithTag ("nebula");
			GameObject[] gameObjectsHL = GameObject.FindGameObjectsWithTag ("hyperlane");
			string file = name.Replace(" ","_");
			string fileName = file+".rsgg";
			StreamWriter sr = File.CreateText(fileName);
			
			sr.WriteLine("{");
			
			//Galaxy Settings
			sr.WriteLine("    \"galaxy\" : {");
			sr.WriteLine("        \"version\" : \""+main.version+"\",");
			sr.WriteLine("        \"name\" : \""+name+"\",");
			sr.WriteLine("        \"random_hyperlanes\" : "+main.autogenHyperlines.ToString().ToLower()+",");
			sr.WriteLine("        \"size\" : "+main.galaxysize+",");
			sr.WriteLine("        \"hl_radius\" : "+main.genhlradius+",");
			sr.WriteLine("        \"hl_maxPerSys\" : "+main.genhlmaxlines+",");
			sr.WriteLine("        \"seed\" : "+main.generationseed+",");
			sr.WriteLine("        \"stars\" : "+main.starstogen+",");
			sr.WriteLine("        \"pnoise\" : "+main.usePerlin.ToString().ToLower()+",");
			sr.WriteLine("        \"pnoise_scale\" : "+main.nscale+",");
			sr.WriteLine("        \"pnoise_freq\" : "+main.nfreq+",");
			sr.WriteLine("        \"pnoise_pers\" : "+main.npers+",");
			sr.WriteLine("        \"png_multiplier\" : "+main.pngmultiplier+",");
			sr.WriteLine("        \"png_randomizer\" : "+main.pngrandomizer+",");
			sr.WriteLine("        \"png_reducer\" : "+main.pngreducer+",");
			sr.WriteLine("        \"minimalSystemDistance\" : "+main.minDist+",");
			
			if(gameObjects.Length > 0)
			{
				sr.WriteLine("        \"systems\" : {");
				for(var i = 0 ; i < gameObjects.Length ; i ++)
				{
					if(gameObjects[i])
					{
						sr.WriteLine("            \""+i+"\" : {");
						if(gameObjects[i].GetComponent<system>().systemName != null && gameObjects[i].GetComponent<system>().systemName != "" && gameObjects[i].GetComponent<system>().systemName != "RandomName" && gameObjects[i].GetComponent<system>().systemName != "random")
						{
							sr.WriteLine("                \"name\" : \""+gameObjects[i].GetComponent<system>().systemName+"\",");
						}	
						else
						{
							sr.WriteLine("                \"name\" : null,");
						}
						sr.WriteLine("                \"position\" : {");
						sr.WriteLine("                    \"x\" : "+gameObjects[i].transform.position.x+",");
						sr.WriteLine("                    \"y\" : "+gameObjects[i].transform.position.z+"");
						sr.WriteLine("                },");
						sr.WriteLine("                \"spawn\" : "+gameObjects[i].GetComponent<system>().isSpawn.ToString().ToLower()+",");
						if(gameObjects[i].GetComponent<system>().isSpawn)
						{
							if(gameObjects[i].GetComponent<system>().initializer != null && gameObjects[i].GetComponent<system>().initializer != "")
								sr.WriteLine("                \"initializer\" : \""+gameObjects[i].GetComponent<system>().initializer+"\",");
							else
								sr.WriteLine("                \"initializer\" : null,");
						}
						else if(gameObjects[i].GetComponent<system>().initializer != null && gameObjects[i].GetComponent<system>().initializer != "")
						{
							sr.WriteLine("                \"initializer\" : \""+gameObjects[i].GetComponent<system>().initializer+"\",");
						}
						else
						{
							sr.WriteLine("                \"initializer\" : null,");
						}
						sr.WriteLine("                \"prevent_hyperlane\" : "+gameObjects[i].GetComponent<system>().prevent.ToString().ToLower()+",");
						sr.WriteLine("                \"id\" : "+gameObjects[i].GetComponent<system>().systemid);
						if(i == gameObjects.Length-1)
							sr.WriteLine("            }");
						else
							sr.WriteLine("            },");
					}
				}
				if(gameObjectsN.Length == 0 && gameObjectsHL.Length == 0)
					sr.WriteLine("        }");
				else
					sr.WriteLine("        },");
			}
			
			if(gameObjectsN.Length > 0)
			{
				sr.WriteLine("        \"nebulas\" : {");
				for(var i = 0 ; i < gameObjectsN.Length ; i ++)
				{
					if(gameObjectsN[i])
					{
						sr.WriteLine("");
						sr.WriteLine("            \""+i+"\" : {");
						sr.WriteLine("                \"name\" : \""+gameObjectsN[i].GetComponent<nebula>().nebulaName+"\",");
						sr.WriteLine("                \"position\" : {");
						sr.WriteLine("                    \"x\" : "+gameObjectsN[i].transform.position.x+",");
						sr.WriteLine("                    \"y\" : "+gameObjectsN[i].transform.position.z+"");
						sr.WriteLine("                },");
						sr.WriteLine("                \"radius\" : "+gameObjectsN[i].GetComponent<nebula>().nebulaRadius+"");
						if(i == gameObjectsN.Length-1)
							sr.WriteLine("            }");
						else
							sr.WriteLine("            },");
					}
				}
				if(gameObjectsHL.Length == 0)
					sr.WriteLine("        }");
				else
					sr.WriteLine("        },");
				sr.WriteLine("");
			}
			
			
			if(gameObjectsHL.Length > 0)
			{
				sr.WriteLine("        \"hyperlanes\" : {");
				for(var i = 0 ; i < gameObjectsHL.Length ; i ++)
				{
					if(gameObjectsHL[i])
					{
						if(i == gameObjectsHL.Length-1)
						{
							if(gameObjectsHL[i].GetComponent<hyperlane>().prevent)
								sr.WriteLine("        \""+i+"\" : { \"from\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link0.systemid+"\", \"to\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link1.systemid+"\", \"mode\" : \"prevent\" }");
							else
								sr.WriteLine("        \""+i+"\" : { \"from\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link0.systemid+"\", \"to\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link1.systemid+"\", \"mode\" : \"add\" }");
						}
						else
						{
							if(gameObjectsHL[i].GetComponent<hyperlane>().prevent)
								sr.WriteLine("        \""+i+"\" : { \"from\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link0.systemid+"\", \"to\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link1.systemid+"\", \"mode\" : \"prevent\" },");
							else
								sr.WriteLine("        \""+i+"\" : { \"from\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link0.systemid+"\", \"to\" : \""+gameObjectsHL[i].GetComponent<hyperlane>().link1.systemid+"\", \"mode\" : \"add\" },");
						}
					}
				}
				sr.WriteLine("        }");
			}
			List<GameObject> gameObjectsSE = main.systemEditors;
			if(gameObjectsSE.Count > 0)
				sr.WriteLine("    },");
			else
				sr.WriteLine("    }");
			//End Galaxy
			
			//Systems Settings
			if(gameObjectsSE.Count > 0)
			{
				sr.WriteLine("    \"customSystems\" : {");
				for(var i = 0 ; i < gameObjectsSE.Count ; i ++)
				{
					systemeditor ses = gameObjectsSE[i].GetComponent<systemeditor>();
					sr.WriteLine("        \""+i+"\" : {");
					sr.WriteLine("            \"name\" : \""+ses.systemName+"\",");
					sr.WriteLine("            \"disableCenterStar\" : "+ses.centerStarDisabled.ToString().ToLower()+",");
					sr.WriteLine("            \"starType\" : \""+ses.starType+"\",");
					sr.WriteLine("            \"starSize\" : "+ses.centerStarSize+",");
					sr.WriteLine("            \"asteroidBelt\" : "+ses.asteroidBelt.ToString().ToLower()+",");
					sr.WriteLine("            \"asteroidBeltRadius\" : "+ses.asteroidBeltRadius+",");
					sr.WriteLine("            \"initializerName\" : \""+ses.systeminitname+"\",");
					sr.WriteLine("            \"systemID\" : "+ses.thisSystem.systemid+",");
					
					int pcnt = 0;
					for(var v = 0 ; v < ses.planets.Count ; v++)
					{
						planet plcp = ses.planets[v].GetComponent<planet>();
						if(plcp.ownerPlanet == null)
						{
							pcnt++;
						}
					}
					
					if(pcnt > 0)
					{
						sr.WriteLine("            \"planets\" : {");
						int jj=0;
						for(var j = 0 ; j < ses.planets.Count ; j ++)
						{
							planet pl = ses.planets[j].GetComponent<planet>();
							if(pl.ownerPlanet == null)
							{
								int mncnt = 0;
								for(var t = 0 ; t < ses.planets.Count ; t++)
								{
									planet plcm = ses.planets[t].GetComponent<planet>();
									if(plcm.ownerPlanet != null)
									{
										if(plcm.ownerPlanet.planetid == pl.planetid)
										{
											mncnt++;
										}
									}
								}
								sr.WriteLine("                \""+jj+"\" : {");
								if(pl.planetName != null && pl.planetName != "" && pl.planetName != "RandomName" && pl.planetName != "random")
									sr.WriteLine("                    \"name\" : \""+pl.planetName+"\",");
								else
									sr.WriteLine("                    \"name\" : null,");
								sr.WriteLine("                    \"class\" : \""+pl.planetClass+"\",");
								sr.WriteLine("                    \"modifier\" : "+pl.planetModifier+",");
								sr.WriteLine("                    \"size\" : "+pl.planetSize+",");
								sr.WriteLine("                    \"orbitDistance\" : "+pl.orbitDistance+",");
								sr.WriteLine("                    \"orbitAngle\" : "+pl.orbitAngle+",");
								sr.WriteLine("                    \"position\" : {");
								sr.WriteLine("                        \"x\" : "+pl.transform.position.x+",");
								sr.WriteLine("                        \"y\" : "+pl.transform.position.z+"");
								sr.WriteLine("                    },");
								sr.WriteLine("                    \"ringed\" : "+pl.hasRing.ToString().ToLower()+",");
								sr.WriteLine("                    \"no_anomaly\" : "+pl.noAnomaly.ToString().ToLower()+",");
								sr.WriteLine("                    \"no_resources\" : "+pl.noResources.ToString().ToLower()+",");
								if(mncnt > 0)
									sr.WriteLine("                    \"home\" : "+pl.isHome.ToString().ToLower()+",");
								else
									sr.WriteLine("                    \"home\" : "+pl.isHome.ToString().ToLower());
								if(mncnt > 0)
								{
									int kk=0;
									sr.WriteLine("                    \"moons\" : {");
									for(var k = 0 ; k < ses.planets.Count ; k++)
									{
										planet plm = ses.planets[k].GetComponent<planet>();
										if(plm.ownerPlanet == pl)
										{
											sr.WriteLine("                        \""+kk+"\" : {");
											if(plm.planetName != null && plm.planetName != "" && plm.planetName != "RandomName" && plm.planetName != "random")
												sr.WriteLine("                            \"name\" : \""+plm.planetName+"\",");
											else
												sr.WriteLine("                            \"name\" : null,");
											sr.WriteLine("                            \"class\" : \""+plm.planetClass+"\",");
											sr.WriteLine("                            \"modifier\" : "+plm.planetModifier+",");
											sr.WriteLine("                            \"size\" : "+plm.planetSize+",");
											sr.WriteLine("                            \"orbitDistance\" : "+plm.orbitDistance+",");
											sr.WriteLine("                            \"orbitAngle\" : "+plm.orbitAngle+",");
											sr.WriteLine("                            \"position\" : {");
											sr.WriteLine("                                \"x\" : "+plm.transform.position.x+",");
											sr.WriteLine("                                \"y\" : "+plm.transform.position.z+"");
											sr.WriteLine("                            },");
											sr.WriteLine("                            \"ringed\" : "+plm.hasRing.ToString().ToLower()+",");
											sr.WriteLine("                            \"no_anomaly\" : "+plm.noAnomaly.ToString().ToLower()+",");
											sr.WriteLine("                            \"no_resources\" : "+plm.noResources.ToString().ToLower()+",");
											sr.WriteLine("                            \"home\" : "+plm.isHome.ToString().ToLower());
											if(kk == mncnt-1)
												sr.WriteLine("                        }");
											else
												sr.WriteLine("                        },");
											kk++;
										}
									}
									sr.WriteLine("                    }");
								}
								if(jj == pcnt-1)
									sr.WriteLine("                }");
								else
									sr.WriteLine("                },");
								jj++;
							}
						}
						sr.WriteLine("            }");
					}
					if(i == gameObjectsSE.Count-1)
						sr.WriteLine("        }");
					else
						sr.WriteLine("        },");
				}
				sr.WriteLine("    }");
			}
			//End Systems
			sr.WriteLine("}");
			sr.Close();	
			main.Log("Project Saved");
		}
		catch
		{
			main.Log("Error while saving project");
		}
	}
	
	void refreshGUI()
	{
		GUILayout.BeginArea(new Rect(10, 10, 200, 1000));
		GUILayout.Label("Save");
		if (GUILayout.Button("Save Project"))
		{
			SaveProject(main.galaxyname);
			refreshProjectList();
		}
		GUILayout.Label("Load");
		loadHyperlanes = GUILayout.Toggle(loadHyperlanes, "Load Hyperlanes");
		for(int i = 0; i < projectList.Count(); i++)
		{
			if (GUILayout.Button(projectList[i]))
			{
				LoadProject(projectList[i]);
				main.editorMode = 0;
			}
		}
		if (GUILayout.Button("Back"))
		{
			main.editorMode = 0;
		}
		if(main.message != null && main.message != "")
		{
			if(GUILayout.Button("Clear Logs"))
			{
				main.ClearLog();
			}
		}
		GUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(200), GUILayout.Height(500));
		GUILayout.Box(main.message);
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		
	}
	
	public void refreshProjectList()
	{
		projectList = new List<string>();
		string[] files = System.IO.Directory.GetFiles("./", "*.rsgg");
		for(int i = 0; i < files.Count(); i++)
		{
			string f = files[i].Replace("./","").Replace(".rsgg","");
			projectList.Add(f);
		}
	}
}
