using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Cloud.Analytics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System;

public class systemeditor : MonoBehaviour {
	public GameObject planet_prefab;
	public int currentMode = 0;
	public planet selectedPlanet;
	public planet prevSelectedPlanet;
	GameObject asteroidBeltRing;
	GameObject centeraxe;
	public system thisSystem;
	Renderer mat;
	public string systemName = "random";
	public string systeminitname;
	public string starType = "random";
	public bool centerStarDisabled = false;
	public bool asteroidBelt = false;
	public int asteroidBeltRadius = 10;
	public int centerStarSize = 20;
	public List<GameObject> planets = new List<GameObject>();
	
	// Use this for initialization
	void Start () {
		centeraxe = transform.FindChild("CenterAxe").gameObject;
		asteroidBeltRing = centeraxe.transform.FindChild("asteroidbelt").gameObject;
		mat = centeraxe.GetComponent<Renderer>();
	}
	
	public void SetForSys(system sys)
	{
		thisSystem = sys;
		systeminitname = "sys_"+sys.systemid+"_init";
		this.gameObject.name = "SystemEditor_"+sys.systemid;
	}
	
	// Update is called once per frame
	void Update () {
		if(centerStarDisabled)
		{
			mat.material.color = Color.gray;
		}
		else
		{
			mat.material.color = Color.yellow;
		}
		if(GUIUtility.hotControl == 0)
		{
			if(Input.GetKeyDown(KeyCode.Mouse0) && main.editorMode == 1)
			{
				switch(currentMode)
				{
					case 2:
						Vector3 poss = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 460));
						GameObject g = (GameObject)Instantiate(planet_prefab, new Vector3(poss.x, 0, poss.z), Quaternion.identity);
						g.transform.SetParent(this.transform, true);
						g.name = "planet_"+thisSystem.systemid;
						g.GetComponent<planet>().ownerSystem = thisSystem.systemid;
						planets.Add(g);
						break;
				}
			}
		}
		if(Input.GetKey(KeyCode.Escape))
		{
			if(currentMode == 0 && main.editorMode == 1)
			{
				if(selectedPlanet)
				{
					selectedPlanet.select(false);
					selectedPlanet = null;
				}
				if(prevSelectedPlanet)
				{
					prevSelectedPlanet.select(false);
					prevSelectedPlanet = null;
				}
			}
			else
			{
				currentMode = 0;
			}
		}
		if(asteroidBelt)
		{
			asteroidBeltRing.GetComponent<CircleDrawer>().radius = asteroidBeltRadius;
		}
		else
		{
			asteroidBeltRing.GetComponent<CircleDrawer>().radius = 0;
		}
		centeraxe.transform.localScale = new Vector3(centerStarSize,1,centerStarSize);
	}
	
    void OnGUI()
    {
		if(main.editorMode == 1 && !main.isGenerating)
		{
			refreshGUISystem();
		}
		else if(main.editorMode == -10 && !main.isGenerating)
		{
			planetclassSelector(selectedPlanet);
		}
		else if(main.editorMode == -11 && !main.isGenerating)
		{
			starclassSelector();
		}
		else if(main.editorMode == -12 && !main.isGenerating)
		{
			planetmodifierSelector(selectedPlanet);
		}
    }
	
	public void save()
	{
		List<GameObject> gameObjects = planets;
		string file = systeminitname.Replace(" ","_");
		string folder = main.galaxyname.Replace(" ","_");
		string fileName = "GeneratedMods\\"+folder+"\\common\\solar_system_initializers\\00_retalyx_"+file+".txt";
		StreamWriter sr = File.CreateText(fileName);
		sr.WriteLine(file+" = {");
		if(systemName != null && systemName != "" && systemName != "RandomName" && systemName != "random")
		{
			sr.WriteLine("    name = \""+systemName+"\"");
		}
		if(starType != null && starType != "" && starType != "RandomName" && starType != "random")
		{
			sr.WriteLine("    class = \""+starType+"\"");
		}
		else
		{
			sr.WriteLine("    class = random");
		}
		if(asteroidBelt)
		{
			sr.WriteLine("    asteroids_distance = "+asteroidBeltRadius);
		}
		sr.WriteLine("");
		sr.WriteLine("    planet = {");
		if(!centerStarDisabled)
		{
			sr.WriteLine("        class = star");
		}
		else
		{
			sr.WriteLine("        class = none");
		}
		sr.WriteLine("        orbit_distance = 0");
		sr.WriteLine("        size = "+centerStarSize);
		sr.WriteLine("    }");
		
		float prevOrbit = 0f;
		float prevOrbitA = 0f;


		int[] distances = new int[gameObjects.Count];
		int[] indexes = new int[gameObjects.Count];
		int arrayind = 0;
		int j = 0;
		foreach(GameObject go in gameObjects)
		{
			int dist = go.GetComponent<planet>().orbitDistance;
			if(dist > 0 && go.GetComponent<planet>().ownerSystem == thisSystem.systemid)
			{
				distances[arrayind] = dist;
				indexes[arrayind] = j;
				arrayind++;
			}
			j++;
		}
		Array.Sort(distances, indexes);
				
				
				
		for(var i = 0 ; i < distances.Length ; i ++)
		{
			int mindDist  = Mathf.Min(distances);
			int ind = indexes[Array.IndexOf(distances, mindDist)];
			distances[Array.IndexOf(distances, mindDist)] = 100000;
			if(gameObjects[ind] && gameObjects[ind].GetComponent<planet>().ownerPlanet == null)
			{
				float newOrbit = gameObjects[ind].GetComponent<planet>().orbitDistance - prevOrbit;
				float newOrbitA = gameObjects[ind].GetComponent<planet>().orbitAngle - prevOrbitA;
				if(newOrbitA < 0)
					newOrbitA = 360 + newOrbitA;
				sr.WriteLine("");
				sr.WriteLine("    planet = {");
				if(gameObjects[ind].GetComponent<planet>().planetName != null && gameObjects[ind].GetComponent<planet>().planetName != "" && gameObjects[ind].GetComponent<planet>().planetName != "RandomName" && gameObjects[ind].GetComponent<planet>().planetName != "random")
					sr.WriteLine("        name = \""+gameObjects[ind].GetComponent<planet>().planetName+"\"");
				string pc = gameObjects[ind].GetComponent<planet>().planetClass;
				
				if(gameObjects[ind].GetComponent<planet>().isHome)
					pc = "ideal_planet_class";
				
				if(pc == "star" || pc == "ideal_planet_class" || pc == "random_non_ideal" || pc == "random" || pc == "random_colonizable" || pc == "random_non_colonizable" || pc == "random_asteroid" || pc == "none")
					sr.WriteLine("        class = "+pc);
				else
					sr.WriteLine("        class = \""+pc+"\"");
				sr.WriteLine("        orbit_distance = "+newOrbit);
				sr.WriteLine("        orbit_angle = "+newOrbitA);
				if(gameObjects[ind].GetComponent<planet>().planetSize > 0)
					sr.WriteLine("        size = "+gameObjects[ind].GetComponent<planet>().planetSize);
				if(gameObjects[ind].GetComponent<planet>().hasRing)
					sr.WriteLine("        has_ring = yes");
				else
					sr.WriteLine("        has_ring = no");
				
				if(gameObjects[ind].GetComponent<planet>().planetModifier == "none")
					sr.WriteLine("        modifier = "+gameObjects[ind].GetComponent<planet>().planetModifier);
				else if(gameObjects[ind].GetComponent<planet>().planetModifier != "random" && gameObjects[ind].GetComponent<planet>().planetModifier != "" && gameObjects[ind].GetComponent<planet>().planetModifier != null)
					sr.WriteLine("        modifier = \""+gameObjects[ind].GetComponent<planet>().planetModifier+"\"");
				
				if(gameObjects[ind].GetComponent<planet>().isHome)
				{
					sr.WriteLine("        tile_blockers = none");
					sr.WriteLine("        home_planet = yes");
				}
				sr.WriteLine("");
				
				if(gameObjects[ind].GetComponent<planet>().noAnomaly || gameObjects[ind].GetComponent<planet>().noResources)
				{
					sr.WriteLine("        init_effect = {");
					if(gameObjects[ind].GetComponent<planet>().noAnomaly)
						sr.WriteLine("            prevent_anomaly = yes");
					if(gameObjects[ind].GetComponent<planet>().noResources)
						sr.WriteLine("            orbital_deposit_tile = { clear_deposits = yes }");
					sr.WriteLine("        }");
				}
				
				if(gameObjects[ind].GetComponent<planet>().isHome)
				{
					sr.WriteLine("        init_effect = {");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no num_adjacent_tiles > 3 }");
					sr.WriteLine("                set_building = \"building_capital_1\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = food");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = minerals");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                }");
					sr.WriteLine("                random_neighboring_tile = {");
					sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                    set_building = \"building_hydroponics_farm_1\"");
					sr.WriteLine("                    add_resource = {");
					sr.WriteLine("                        resource = food");
					sr.WriteLine("                        amount = 1");
					sr.WriteLine("                        replace = yes");
					sr.WriteLine("                    }");		
					sr.WriteLine("                }");
					sr.WriteLine("                random_neighboring_tile = {");
					sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                    set_building = \"building_power_plant_1\"");
					sr.WriteLine("                    add_resource = {");
					sr.WriteLine("                        resource = energy");
					sr.WriteLine("                        amount = 1");
					sr.WriteLine("                        replace = yes");
					sr.WriteLine("                    }");
					sr.WriteLine("                }");
					sr.WriteLine("                random_neighboring_tile = {");
					sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                    set_building = \"building_power_plant_1\"");
					sr.WriteLine("                    add_resource = {");
					sr.WriteLine("                        resource = energy");
					sr.WriteLine("                        amount = 1");
					sr.WriteLine("                        replace = yes");
					sr.WriteLine("                    }");
					sr.WriteLine("                }");
					sr.WriteLine("                random_neighboring_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_building = \"building_mining_network_1\"");
					sr.WriteLine("                    add_resource = {");
					sr.WriteLine("                        resource = minerals");
					sr.WriteLine("                        amount = 1");
					sr.WriteLine("                        replace = yes");
					sr.WriteLine("                    }");
					sr.WriteLine("                }");
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_blocker = \"tb_failing_infrastructure\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = engineering_research");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_blocker = \"tb_failing_infrastructure\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("            	      resource = society_research");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_blocker = \"tb_failing_infrastructure\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = physics_research");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_blocker = \"tb_decrepit_dwellings\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = energy");
					sr.WriteLine("                    amount = 2");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");					
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_blocker = \"tb_decrepit_dwellings\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = food");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");					
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                set_blocker = \"tb_decrepit_dwellings\"");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = energy");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");
					sr.WriteLine("            }");
					sr.WriteLine("            random_tile = {");
					sr.WriteLine("                limit = { has_blocker = no has_building = no }");
					sr.WriteLine("                add_resource = {");
					sr.WriteLine("                    resource = food");
					sr.WriteLine("                    amount = 1");
					sr.WriteLine("                    replace = yes");
					sr.WriteLine("                }");
					sr.WriteLine("            }");
					sr.WriteLine("        }");
				}

				int[] distancesM = new int[gameObjects.Count];
				int[] indexesM = new int[gameObjects.Count];
				int arrayindM = 0;
				int jM = 0;
				foreach(GameObject goM in gameObjects)
				{
					int dist = goM.GetComponent<planet>().orbitDistance;
					if(dist > 0 && goM.GetComponent<planet>().ownerSystem == thisSystem.systemid && goM.GetComponent<planet>().ownerPlanet == gameObjects[ind].GetComponent<planet>())
					{
						distancesM[arrayindM] = dist;
						indexesM[arrayindM] = j;
						arrayindM++;
					}
					jM++;
				}
				Array.Sort(distancesM, indexesM);
				
				float prevOrbitM = 0f;
				float prevOrbitAM = 0f;
				for(var m = 0 ; m < distancesM.Length ; m ++)
				{
					int mindDistM  = Mathf.Min(distancesM);
					int indM = indexes[Array.IndexOf(distancesM, mindDistM)];
					distancesM[Array.IndexOf(distancesM, mindDistM)] = 100000;
					if(gameObjects[indM] && gameObjects[indM].GetComponent<planet>().ownerPlanet == gameObjects[ind].GetComponent<planet>())
					{
						float newOrbitM = gameObjects[indM].GetComponent<planet>().orbitDistance - prevOrbitM;
						float newOrbitAM = gameObjects[indM].GetComponent<planet>().orbitAngle - prevOrbitAM;
						if(newOrbitAM < 0)
							newOrbitAM = 360 + newOrbitAM;
						
						sr.WriteLine("");
						sr.WriteLine("        moon = {");
						if(gameObjects[indM].GetComponent<planet>().planetName != null && gameObjects[indM].GetComponent<planet>().planetName != "" && gameObjects[indM].GetComponent<planet>().planetName != "RandomName" && gameObjects[indM].GetComponent<planet>().planetName != "random")
							sr.WriteLine("            name = \""+gameObjects[indM].GetComponent<planet>().planetName+"\"");
						string pcm = gameObjects[indM].GetComponent<planet>().planetClass;
						if(pcm == "star" || pcm == "ideal_planet_class" || pcm == "random_non_ideal" || pcm == "random" || pcm == "random_colonizable" || pcm == "random_non_colonizable" || pcm == "random_asteroid" || pcm == "none")
							sr.WriteLine("            class = "+pcm);
						else
							sr.WriteLine("            class = \""+pcm+"\"");
						sr.WriteLine("            orbit_distance = "+newOrbitM);
						sr.WriteLine("            orbit_angle = "+newOrbitAM);
						if(gameObjects[indM].GetComponent<planet>().planetSize > 0)
							sr.WriteLine("            size = "+gameObjects[indM].GetComponent<planet>().planetSize);
						if(gameObjects[indM].GetComponent<planet>().hasRing)
							sr.WriteLine("            has_ring = yes");
						else
							sr.WriteLine("            has_ring = no");
				
						if(gameObjects[indM].GetComponent<planet>().planetModifier == "none")
							sr.WriteLine("            modifier = "+gameObjects[indM].GetComponent<planet>().planetModifier);
						else if(gameObjects[indM].GetComponent<planet>().planetModifier != "random" && gameObjects[indM].GetComponent<planet>().planetModifier != "" && gameObjects[indM].GetComponent<planet>().planetModifier != null)
							sr.WriteLine("            modifier = \""+gameObjects[indM].GetComponent<planet>().planetModifier+"\"");
				
						if(gameObjects[indM].GetComponent<planet>().isHome)
						{
							sr.WriteLine("            tile_blockers = none");
							sr.WriteLine("            home_planet = yes");
						}
						sr.WriteLine("");
						
						if(gameObjects[indM].GetComponent<planet>().noAnomaly || gameObjects[indM].GetComponent<planet>().noResources)
						{
							sr.WriteLine("            init_effect = {");
							if(gameObjects[indM].GetComponent<planet>().noAnomaly)
								sr.WriteLine("                prevent_anomaly = yes");
							if(gameObjects[indM].GetComponent<planet>().noResources)
								sr.WriteLine("                orbital_deposit_tile = { clear_deposits = yes }");
							sr.WriteLine("            }");
						}
				
						if(gameObjects[indM].GetComponent<planet>().isHome)
						{
							sr.WriteLine("            init_effect = {");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no num_adjacent_tiles > 3 }");
							sr.WriteLine("                    set_building = \"building_capital_1\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = food");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = minerals");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                    }");
							sr.WriteLine("                    random_neighboring_tile = {");
							sr.WriteLine("                        limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                        set_building = \"building_hydroponics_farm_1\"");
							sr.WriteLine("                        add_resource = {");
							sr.WriteLine("                            resource = food");
							sr.WriteLine("                            amount = 1");
							sr.WriteLine("                            replace = yes");
							sr.WriteLine("                        }");		
							sr.WriteLine("                    }");
							sr.WriteLine("                    random_neighboring_tile = {");
							sr.WriteLine("                        limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                        set_building = \"building_power_plant_1\"");
							sr.WriteLine("                        add_resource = {");
							sr.WriteLine("                            resource = energy");
							sr.WriteLine("                            amount = 1");
							sr.WriteLine("                            replace = yes");
							sr.WriteLine("                        }");
							sr.WriteLine("                    }");
							sr.WriteLine("                    random_neighboring_tile = {");
							sr.WriteLine("                        limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                        set_building = \"building_power_plant_1\"");
							sr.WriteLine("                        add_resource = {");
							sr.WriteLine("                            resource = energy");
							sr.WriteLine("                            amount = 1");
							sr.WriteLine("                            replace = yes");
							sr.WriteLine("                        }");
							sr.WriteLine("                    }");
							sr.WriteLine("                    random_neighboring_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_building = \"building_mining_network_1\"");
							sr.WriteLine("                        add_resource = {");
							sr.WriteLine("                            resource = minerals");
							sr.WriteLine("                            amount = 1");
							sr.WriteLine("                            replace = yes");
							sr.WriteLine("                        }");
							sr.WriteLine("                    }");
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_blocker = \"tb_failing_infrastructure\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = engineering_research");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_blocker = \"tb_failing_infrastructure\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                	      resource = society_research");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_blocker = \"tb_failing_infrastructure\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = physics_research");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_blocker = \"tb_decrepit_dwellings\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = energy");
							sr.WriteLine("                        amount = 2");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");					
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_blocker = \"tb_decrepit_dwellings\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = food");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");					
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    set_blocker = \"tb_decrepit_dwellings\"");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = energy");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");
							sr.WriteLine("                }");
							sr.WriteLine("                random_tile = {");
							sr.WriteLine("                    limit = { has_blocker = no has_building = no }");
							sr.WriteLine("                    add_resource = {");
							sr.WriteLine("                        resource = food");
							sr.WriteLine("                        amount = 1");
							sr.WriteLine("                        replace = yes");
							sr.WriteLine("                    }");
							sr.WriteLine("                }");
							sr.WriteLine("            }");
						}
						sr.WriteLine("        }");
						prevOrbitM = gameObjects[indM].GetComponent<planet>().orbitDistance;
						prevOrbitAM = gameObjects[indM].GetComponent<planet>().orbitAngle;
					}
				}
				sr.WriteLine("    }");
				prevOrbit = gameObjects[ind].GetComponent<planet>().orbitDistance;
				prevOrbitA = gameObjects[ind].GetComponent<planet>().orbitAngle;
			}
		}
		sr.WriteLine("}");
		sr.Close();	
	}
	
	void back()
	{
		string file = systeminitname.Replace(" ","_");
		thisSystem.initializer = file;
		thisSystem.systemName = systemName;
		main.editorMode = 0;
		main.GalaxyEditor.SetActive(true);
		this.gameObject.SetActive(false);
	}
	
	void delete()
	{
		thisSystem.initializer = "";
		thisSystem.systemName = "";
		main.editorMode = 0;
		main.GalaxyEditor.SetActive(true);
		Destroy(this.gameObject);
	}
	
	private void refreshGUISystem()
	{
		//Left Panel
		GUILayout.BeginArea(new Rect(10, 10, 200, 1000));
			
		GUILayout.Label("Galaxy Generator by Retalyx V"+main.version);
		if (GUILayout.Button("Donate"))
		{
			Camera.main.SendMessage("donate");
		}
		
		GUILayout.Label("System Name:");
		systemName = GUILayout.TextField(systemName);
		GUILayout.Label("System Initializer Name:\n"+systeminitname);
		
		GUILayout.Label("System Star Type:");
		GUILayout.BeginHorizontal("box");
		starType = GUILayout.TextField(starType);
		if (GUILayout.Button("..."))
		{
			main.editorMode = -11;
		}
		GUILayout.EndHorizontal();
		centerStarDisabled = GUILayout.Toggle(centerStarDisabled, "Disable Center Star");
		
		if(!centerStarDisabled)
		{
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Center Star Size:");
			centerStarSize = int.Parse(GUILayout.TextField(centerStarSize+"", 3));
			GUILayout.EndHorizontal();
		}
		
		asteroidBelt = GUILayout.Toggle(asteroidBelt, "Asteroid Belt");
		if(asteroidBelt)
		{
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Radius:");
			asteroidBeltRadius = int.Parse(GUILayout.TextField(asteroidBeltRadius+""));
			GUILayout.EndHorizontal();
		}
				
		// GUILayout.BeginHorizontal("box");
		// if (GUILayout.Button("Save to file"))
		// {
			// save();
		// }
		// if (GUILayout.Button("Back"))
		if (GUILayout.Button("Save"))
		{
			back();
		}
		GUI.color = Color.red;
		if (GUILayout.Button("Delete System"))
		{
			delete();
		}
		GUI.color = Color.white;
		// GUILayout.EndHorizontal();
		
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
		
		//Right Panel}
		string modeText = "No Mode";
		GUILayout.BeginArea(new Rect(Screen.width-210, 10, 200, 1000));
		switch(currentMode)
		{				
			case 1:
				modeText = "Move";
				break;
				
			case 2:
				modeText = "Create Planet";
				break;
				
			case 3:
				modeText = "Delete Planet";
				break;
				
			default:
				modeText = "No Mode";
				break;
		}
		GUILayout.Label("Current Mode: " + modeText);
		if (GUILayout.Button("Move"))
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
			
		GUILayout.BeginHorizontal("box");
		if (GUILayout.Button("Create Planet"))
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
		if (GUILayout.Button("Delete Planet"))
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
		GUILayout.EndHorizontal();
		
		if(selectedPlanet)
		{
			GUILayout.Label("Selected Planet Infos");
			GUILayout.Label("Name:");
			selectedPlanet.planetName = GUILayout.TextField(selectedPlanet.planetName);
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Type:");
			selectedPlanet.planetClass = GUILayout.TextField(selectedPlanet.planetClass+"");
			if (GUILayout.Button("..."))
			{
				main.editorMode = -10;
			}
			GUILayout.EndHorizontal();
			// GUILayout.Label("Planet Modifier:");
			// GUILayout.BeginHorizontal("box");
			// selectedPlanet.planetModifier = GUILayout.TextField(selectedPlanet.planetModifier+"");
			// if (GUILayout.Button("..."))
			// {
				// main.editorMode = -12;
			// }
			// GUILayout.EndHorizontal();
			GUILayout.Label("Position");
			GUILayout.Label("X: "+selectedPlanet.transform.position.x);
			GUILayout.Label("Y: "+selectedPlanet.transform.position.z);
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Size:");
			selectedPlanet.planetSize = int.Parse(GUILayout.TextField(selectedPlanet.planetSize+"", 5));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Orbit Radius:");
			selectedPlanet.orbitDistance = int.Parse(GUILayout.TextField(selectedPlanet.orbitDistance+"", 5));
			GUILayout.EndHorizontal();
			GUILayout.Label("Orbit Angle: "+selectedPlanet.orbitAngle);
			selectedPlanet.hasRing = GUILayout.Toggle(selectedPlanet.hasRing, "Has Ring");
			selectedPlanet.noAnomaly = GUILayout.Toggle(selectedPlanet.noAnomaly, "No Anomaly");
			selectedPlanet.noResources = GUILayout.Toggle(selectedPlanet.noResources, "No Orbital Resources");
			selectedPlanet.isHome = GUILayout.Toggle(selectedPlanet.isHome, "Home Planet");
			if(selectedPlanet.ownerPlanet == null)
			{
				if (GUILayout.Button("Create Moon"))
				{
					Vector3 poss = selectedPlanet.transform.position + new Vector3(-10,0,0);
					GameObject g = (GameObject)Instantiate(planet_prefab, new Vector3(poss.x, 0, poss.z), Quaternion.identity);
					g.transform.SetParent(this.transform, true);
					g.name = "moon_"+thisSystem.systemid;
					g.GetComponent<planet>().ownerSystem = thisSystem.systemid;
					g.GetComponent<planet>().ownerPlanet = selectedPlanet;
					planets.Add(g);
				}
			}
		}
        GUILayout.EndArea();
	}
	
	public void planetclassSelector(planet p)
	{	
        GUILayout.BeginArea(main.scrollerRect);
		main.scrollPos = GUILayout.BeginScrollView(main.scrollPos, GUILayout.Width(main.scrollerRect.width), GUILayout.Height(main.scrollerRect.height));
		if(GUILayout.Button("Random"))
		{
			p.planetClass = "random";
			main.editorMode = 1;
		}
		for(int x = 0; x < main.starClassPresets.Count; x++)
		{
			if(GUILayout.Button(main.starClassPresetsN[x]))
			{
				p.planetClass = main.starClassPresets[x];
				main.editorMode = 1;
			}
		}
		for(int x = 0; x < main.planetClassPresets.Count; x++)
		{
			if(GUILayout.Button(main.planetClassPresetsN[x]))
			{
				p.planetClass = main.planetClassPresets[x];
				main.editorMode = 1;
			}
		}
		GUILayout.EndScrollView();
        GUILayout.EndArea();
	}
	
	public void starclassSelector()
	{	
        GUILayout.BeginArea(main.scrollerRect);
		main.scrollPos = GUILayout.BeginScrollView(main.scrollPos, GUILayout.Width(main.scrollerRect.width), GUILayout.Height(main.scrollerRect.height));
		if(GUILayout.Button("Random"))
		{
			starType = "random";
			main.editorMode = 1;
		}
		for(int x = 0; x < main.starClassPresets.Count; x++)
		{
			if(GUILayout.Button(main.starClassPresetsN[x]))
			{
				starType = main.starClassPresets[x];
				main.editorMode = 1;
			}
		}
		GUILayout.EndScrollView();
        GUILayout.EndArea();
	}
	
	public void planetmodifierSelector(planet p)
	{	
        GUILayout.BeginArea(main.scrollerRect);
		main.scrollPos = GUILayout.BeginScrollView(main.scrollPos, GUILayout.Width(main.scrollerRect.width), GUILayout.Height(main.scrollerRect.height));
		if(GUILayout.Button("Random"))
		{
			p.planetModifier = "random";
			main.editorMode = 1;
		}
		for(int x = 0; x < main.planetModifierPresets.Count; x++)
		{
			if(GUILayout.Button(main.planetModifierPresetsN[x]))
			{
				p.planetModifier = main.planetModifierPresets[x];
				main.editorMode = 1;
			}
		}
		GUILayout.EndScrollView();
        GUILayout.EndArea();
	}
}
