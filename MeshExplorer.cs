
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Tools
{
    enum Selecter
    {
        Все,
        Да,
        Нет 
    }

    public class MeshExplorer : EditorWindow
    {
        GUILayoutOption[] options = {GUILayout.Width(150)};
        
        GUILayoutOption[] options2 = {GUILayout.Width(150),GUILayout.MaxWidth(150)};
        
        Dictionary<Mesh,MeshData> meshes = new Dictionary<Mesh,MeshData>();
        IEnumerable<KeyValuePair<Mesh,MeshData>> meshesData;
        int i = 0;
        int toolbar = 1;

        string[] headerNames = {"Название","Вертексов","Трисов","Использований","Сумма вертексов","Readable","UV lightmap"};
        Color defaultColor = new Vector4(1f,1f,1f,1f);
        string searchName = "";
        int minVert = 0;
        int maxVert = 0;
        int minTris = 0;
        int maxTris = 0;

        int minUses = 0;
        int maxUses = 0;

        int minVertSum = 0;
        int maxVertSum = 0;
        Selecter readable;
        Selecter generateUV;

        int order = 4;
        bool orderDes = true;


        Vector2 scrollPos;
        [MenuItem("Tools/MeshExplorer")]
        public static void ShowWindow()
        {
            EditorWindow editorWindow = GetWindow<MeshExplorer>("MeshExplorer");
            editorWindow.minSize = new Vector2(1250,300);
            editorWindow.autoRepaintOnSceneChange = true;          
        }

        void OnGUI()
        {
            UpdateMeshes();
            Header();
            Filters();
            PrintTable();
        }

        void UpdateMeshes()
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(15));
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();
            if(GUILayout.Button("Обновить Меши",GUILayout.Width(Screen.width),GUILayout.Height(30)) )
            {
                meshes = new Dictionary<Mesh,MeshData>();
                ModelImporter modelImporter;
                foreach(MeshFilter filter in FindObjectsOfType<MeshFilter>())
                {
                    if(filter.sharedMesh!=null)
                    {
                        modelImporter = ModelImporter.GetAtPath( AssetDatabase.GetAssetPath(filter.sharedMesh) ) as ModelImporter;
                        if(modelImporter!=null)
                        {
                            if(!meshes.ContainsKey(filter.sharedMesh))
                            {                    
                                meshes.Add(filter.sharedMesh, new MeshData(modelImporter) );
                            }  
                            else
                            {
                                meshes[filter.sharedMesh].amount++;
                            }     
                        }
                    }                         
                } 

            }
            EditorGUILayout.BeginVertical(GUILayout.Height(30));
            GUILayout.Space(30);
            EditorGUILayout.EndVertical();
        }

        void Header()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("N", options);
            DrawHeaders();
            GUILayout.EndHorizontal();
        }

        void DrawHeaders()
        {
            for(int ind = 0 ; ind < 7; ind++)
            {
                ChangeColor(ind);
                if ( GUILayout.Button(headerNames[ind], options) )
                {
                    ChangeOrder(ind);
                }    
            }
            GUI.color = defaultColor;           
        }

        void ChangeColor(int index)
        {
            if(order==index)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = defaultColor; 
            }
        }

        void DiscardAll()
        {
            searchName = "";
            minVert = 0;
            maxVert = 0;

            minTris = 0;
            maxTris = 0;

            minUses = 0;
            maxUses = 0;

            minVertSum = 0;
            maxVertSum = 0;
            
            readable = Selecter.Все;
            generateUV = Selecter.Все;  
        }

        void Filters()
        {
            GUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginHorizontal(options2);
            if( GUILayout.Button("Сбросить", options2) )
            {
                DiscardAll();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(options2);
            searchName = GUILayout.TextField(searchName,25,options);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(options2);
            minVert = EditorGUILayout.IntField(minVert);  
            EditorGUILayout.LabelField("-",GUILayout.Width(10));
            maxVert = EditorGUILayout.IntField(maxVert);
            if(GUILayout.Button("X",GUILayout.Width(20)) )
            {
                minVert = 0;
                maxVert = 0;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(options2);
            minTris = EditorGUILayout.IntField(minTris);
            EditorGUILayout.LabelField("-",GUILayout.Width(10));
            maxTris = EditorGUILayout.IntField(maxTris);
            if(GUILayout.Button("X",GUILayout.Width(20)) )
            {
                minTris = 0;
                maxTris = 0;
            }
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.BeginHorizontal(options2);
            minUses = EditorGUILayout.IntField(minUses);
            EditorGUILayout.LabelField("-",GUILayout.Width(10));
            maxUses = EditorGUILayout.IntField(maxUses);
            if(GUILayout.Button("X",GUILayout.Width(20)) )
            {
                minUses = 0;
                maxUses = 0;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(options2);
            minVertSum = EditorGUILayout.IntField(minVertSum);
            EditorGUILayout.LabelField("-",GUILayout.Width(10));
            maxVertSum = EditorGUILayout.IntField(maxVertSum);
            if(GUILayout.Button("X",GUILayout.Width(20)) )
            {
                minVertSum = 0;
                maxVertSum = 0;
            }
            EditorGUILayout.EndHorizontal();

            
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(150));
            readable = (Selecter)EditorGUILayout.EnumPopup(readable,options);
            generateUV = (Selecter)EditorGUILayout.EnumPopup(generateUV,options); 
            EditorGUILayout.EndHorizontal();   

            GUILayout.EndHorizontal();
        }

        

        void PrintTable()
        {
            if(meshes.Keys.Count>0)
            {
                i=1;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                Sorting();
                Ordering();
                foreach (var mesh in meshesData)
                {
                    CreateLine(mesh.Key, mesh.Value);
                    i++;
                }
                GUILayout.EndScrollView();    
            }
        }

        void ChangeOrder(int id)
        {
            if(order!=id)
            {
                order = id;
                orderDes = true;
            }
            else
            {
                orderDes = !orderDes;    
            }       
        }

        void Sorting()
        {
            meshesData = from m in meshes 
            select m;

            if(!searchName.Equals(""))
            {
                meshesData = meshesData.Where(m => m.Key.name.Contains(searchName) );
            }

            if(!minVert.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Key.vertexCount >= minVert);
            }
            if(!maxVert.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Key.vertexCount <= maxVert);
            }

            if(!minTris.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Key.triangles.Length/3 >= minTris);
            }
            if(!maxTris.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Key.triangles.Length/3 <= maxTris);
            }

            if(!minUses.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Value.amount  >= minUses);
            }
            if(!maxUses.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Value.amount <= maxUses);
            }

            if(!minVertSum.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Value.amount*m.Key.vertexCount  >= minVertSum);
            }
            if(!maxVertSum.Equals(0))
            {
                meshesData = meshesData.Where(m => m.Value.amount*m.Key.vertexCount <= maxVertSum);
            }

            if(readable!=Selecter.Все)
            {
                if(readable==Selecter.Да)
                {
                    meshesData = meshesData.Where(m => m.Value.modelImporter.isReadable==true); 
                }
                else
                {
                    meshesData = meshesData.Where(m => m.Value.modelImporter.isReadable==false); 
                }      
            }

            if(generateUV!=Selecter.Все)
            {
                if(generateUV==Selecter.Да)
                {
                    meshesData = meshesData.Where(m => m.Value.modelImporter.generateSecondaryUV==true); 
                }
                else
                {
                    meshesData = meshesData.Where(m => m.Value.modelImporter.generateSecondaryUV==false); 
                }      
            }            
        }

        void Ordering()
        {
            switch(order)
            {
                case 0:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Key.name) : meshesData.OrderBy(u=>u.Key.name);
                break;
                case 1:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Key.vertexCount) : meshesData.OrderBy(u=>u.Key.vertexCount);
                break;
                case 2:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Key.triangles.Length/3) : meshesData.OrderBy(u=>u.Key.triangles.Length/3);
                break;
                case 3:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Value.amount) : meshesData.OrderBy(u=>u.Value.amount);
                break;
                case 4:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Value.amount*u.Key.vertexCount) : meshesData.OrderBy(u=>u.Value.amount*u.Key.vertexCount);
                break;
                case 5:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Value.modelImporter.isReadable) : meshesData.OrderBy(u=>u.Value.modelImporter.isReadable);
                break;
                case 6:
                meshesData = orderDes ? meshesData.OrderByDescending(u=>u.Value.modelImporter.generateSecondaryUV) : meshesData.OrderBy(u=>u.Value.modelImporter.generateSecondaryUV);
                break;
            }

        }

        void CreateLine(Mesh mesh,  MeshData data)
        {
            GUIStyle styleCenter = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
            GUILayout.BeginHorizontal("box");
            
            GUILayout.Label(i.ToString(),styleCenter,options);
            if( GUILayout.Button(mesh.name,options) )
            {
                EditorGUIUtility.PingObject(mesh);
            }
            GUILayout.Label(mesh.vertexCount.ToString(),styleCenter,options);
            GUILayout.Label( (mesh.triangles.Length/3).ToString(),styleCenter ,options);
            GUILayout.Label(data.amount.ToString(),styleCenter,options);
            GUILayout.Label((mesh.vertexCount*data.amount).ToString(),styleCenter ,options);

            data.modelImporter.isReadable = GUILayout.Toggle(data.modelImporter.isReadable,"Readable",options);      
            data.modelImporter.generateSecondaryUV = GUILayout.Toggle(data.modelImporter.generateSecondaryUV,"Generate UV",options);
            
            GUILayout.EndHorizontal();    
        }

        class MeshData
        {
            public int amount;
            public ModelImporter modelImporter;

            public MeshData(ModelImporter model)
            {
                amount = 1;
                modelImporter = model;
            }
        }

    }

}
