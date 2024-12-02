Version 1.5.6

- Additional demo scenes  http://kripto289.com/AssetStore/WaterSystem/DemoScenes_1.5/
- My email is "kripto289@gmail.com"
- Discord channel https://discord.gg/GUUZ9D96Uq (you can get all new changes/fixes/features in the discord channel. The asset store version will only receive major updates)







-----------------------------------  WATER FIRST STEPS ------------------------------------------------------------------------------------------------------------

               1) Right click in hierarchy -> Effects -> Water system
               2) See the description of each setting: just click the help box with the symbol "?" or go over the cursor to any setting to see a text description. 

--------------------------------------------------------------------------------------------------------------------------------------------------------------------








-----------------------------------  DEMO SCENE CORRECT SETTINGS -----------------------------------------------------------------------------------------------
1) Use linear color space. Edit-> Project settings -> Player -> Other settings -> Color space -> Linear
If you use gamma space, then you need to change light intensity and water transparent/turbidity for better looking.
2) Import "cinemachine" (for camera motion)
Window -> Package Manager -> click button bellow "packages" tab -> select "All Packages" or "Packages: Unity registry" -> Cinemachine -> "Install"
----------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------- USING FLOWING EDITOR ---------------------------------------------------------------------------------------------------------
1) Click the "Flowmap Painter" button
2) Set the "Flowmap area position" and "Area Size" parameters. You must draw flowmap in this area!
3) Press and hold the left mouse button to draw on the flowmap area.
4) Use the "control" (ctrl) button + left mouse to erase mode.
5) Use the mouse wheel to change the brush size.
6) Press the "Save All" button.
7) All changes will be saved in the folder "Assets\KriptoFX\WaterSystem\WaterResources\Resources\SavedData\WaterID", so be careful and don't remove it.
You can see the current waterID under section "water->rendering tab". It's look like a "Water unique ID : Beach Rocky.M8W3ER5V"
----------------------------------------------------------------------------------------------------------------------------------------------------------------



----------------------------------- USING FLUIDS SIMULATION -------------------------------------------------------------------------------------------------
Fluids simulation calculate dynamic flow around static objects only.
1) draw the flow direction on the current flowmap (use flowmap painter)
2) save flowmap
3) press the button "Bake Fluids Obstacles"
-------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------- USING SHORELINE EDITOR ---------------------------------------------------------------------------------------------------------
1) Click the "Edit mode" button
2) Click the "Add Wave" button OR you can add waves to the mouse cursor position using the "Insert" key button. For removal, select a wave and press the "Delete" button.
3) You can use move/rotate/scale as usual for any other game object. 
4) Save all changes.
5) All changes will be saved in the folder "Assets\KriptoFX\WaterSystem\WaterResources\Resources\SavedData\WaterID", so be careful and don't remove it.
You can see the current waterID under section "water->rendering tab". It's look like a "Water unique ID : Beach Rocky.M8W3ER5V"
----------------------------------------------------------------------------------------------------------------------------------------------------------------



----------------------------------- USING RIVER SPLINE EDITOR ---------------------------------------------------------------------------------------------------------
1) In this mode, a river mesh is generated using splines (control points).
Press the button "Add River" and left click on your ground and set the starting point of your river
2) Press 
SHIFT + LEFT click to add a new point.
Ctrl + Left click deletes the selected point.
Use "scale tool" (or R button) to change the river width
3) A minimum of 3 points is required to create a river. Place the points approximately at the same distance and avoid strong curvature of the mesh 
(otherwise you will see red intersections gizmo and artifacts)
4) Press "Save Changes"
----------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------- USING ADDITIONAL FEATURES ---------------------------------------------------------------------------------------------------------
1) You can use the "water depth mask" feature (used for example for ignoring water rendering inside a boat). 
Just create a mesh mask and use shader "KriptoFX/Water/KW_WaterHoleMask"
2) For buoyancy, add the script "KW_Buoyancy" to your object with rigibody. 
3) For compatibility with third-party assets (Enviro/Azure/Atmospheric height fog/etc) use WaterSystem -> Rendering -> Third-party fog support
----------------------------------------------------------------------------------------------------------------------------------------------------------------



----------------------------------- WATER API --------------------------------------------------------------------------------------------------------------------------
1) To get the water position/normal (for example for bouyancy) use follow code:

var waterSurfaceData = WaterSystem.GetWaterSurfaceData(position); //Used for water physics. It check all instances. 
if (waterSurfaceData.IsActualDataReady) //checking if the surface data is ready. Since I use asynchronous updating, the data may be available with a delay, so the first frame can be null. 
{
    var waterPosition = waterSurfaceData.Position;
    var waterNormal = waterSurfaceData.Normal;
} 
2) if you want to manually synchronize the time for all clients over the network, use follow code:
_waterInstance.UseNetworkTime = true;
_waterInstance.NetworkTime = ...  //your time in seconds

3) WaterInstance.IsWaterRenderingActive = true/false;   //You can manually control the rendering of water (software occlusion culling)
4) WaterInstance.WorldSpaceBounds   //world space bounds of the current quadtree mesh/custom mesh/river 
5) WaterInstance.IsCameraUnderwater() //check if the current rendered camera intersect underwater volume
6) WaterInstance.IsPositionUnderWater(position) or WaterInstance.IsSphereUnderWater(position, radius) //Check if the current world space position/sphere is under water. 
For example, you can detect if your character enters the water to like triggering a swimming state.
7) Example of adding shoreline waves in realtime

var data =  WaterInstance.Settings.ShorelineWaves = ScriptableObject.CreateInstance<ShorelineWavesScriptableData>();
for(int i = 0; I < 100; i++)
{
  var wave = new ShorelineWave(typeID: 0, position, rotation, scale, timeOffset, flip);
  wave.UpdateMatrix();
  data.Waves.Add(wave);
}
WaterInstance.Settings.ShorelineWaves = data;
----------------------------------------------------------------------------------------------------------------------------------------------------------------




Other resources: 
Galleon https://sketchfab.com/Harry_L
Shark https://sketchfab.com/Ravenloop
Pool https://sketchfab.com/aurelien_martel
