# 3rd Person Camera
Copyright © 2015-2024 
Thomas Enzenebner

## Version 1.1

Thank you for buying the 3rd Person Camera Asset!

If you have any questions, suggestions, comments or feature requests, please send
an [email](mailto:support@enzenebner.com).

Visit the forum: https://forum.unity.com/threads/3rd-person-camera-emulating-the-world-of-warcraft-camera-with-smart-pivot-and-thickness-checks.383754/#post-4356535

Beta access: https://www.enzenebner.com/beta

------------------------------

## Upgrade Guide
### Update to 1.0.8 from previous versions

Before upgrading, remove the old "3rdPersonCamera" folder before importing as some files were moved.


### Update to 1.0.5.8 from previous versions


When upgrading from a previous version add the relevant CameraInputSampling script to the game object. 
The scripts will throw an error when the CameraInputSampling components are not found and CustomInput is not activated.

------------------------------
## How To use 3rd Person Camera

There are several prebuilt camera gameobjects in the "Prefabs" folder to start from. These are:
- 3rdPersonCamera_Basic: Most basic setup to enable clipping logic
- 3rdPersonCamera_FreeForm: 360 Orbit Camera with distance.
- 3rdPersonCamera_FreeFormAndTargeting: Orbit camera with lookAt/targeting support
- 3rdPersonCamera_Follow: smoothly aligns itself to the transform.forward of the target
- 3rdPersonCamera_OverTheShoulder: FreeForm and over the shoulder camera from games like The Division

In this asset there are 7 demo scenes to see how it's done.
- Adventure
- Ball
- Car
- Soulslike
- Shooter
- Follow
- Freeform 

### Base components

There are 6 basic components which usually can be put safely on the main camera gameobject. (excluding Targetable)
- `CameraController`
	The main script and always needed for the following scripts
- `Freeform`
	Orbital script to look around 360 degrees with a given distance and user input
- `Follow`
	Enables automatic camera movement without user input
- `OverTheShoulder`
	Using this in default mode, the camera is able to move left or right with a given input without changing the direction
	to the target.
- `DisableFollow`
	Helper script to disable follow on certain conditions	
- `LockOnTarget`
	Enables locking on gameobjects with a Targetable component. LockOn will keep the target in the middle 
	of the screen without user input.
- `Targetable`
	This component makes the gameobject available for the LockOnTarget component

------------------------------

## Input System
### New Input system Setup

Step 1:
- Navigate to "Project Settings" -> "Player"
- In "Other Settings" -> "Active Input Handling"
  select "Both" or "Input System Package (New)"
- When prompted to restart, do so

Step 2:
- Go to Window->Package Manager
- Select Packages: Unity Registry
- Search for "Input System" (version 1.7+) and click install
  (If you don't see a version, there's a small arrow to the left of "Input System" which expands the versions)

Step 3:
- "Input System" will automatically code generate `TPC_Input.cs` in `3rdPersonCamera\Settings\Input\`
- It's possible Unity hasn't registered the file correctly which means another Unity restart.
- In case the restart also doesn't generate the file, try opening the `TPC_Input` input schema, trigger a change and 
resave.

### New Input system Usage

For the new input system, only a single component is used `CameraInputSampling`.

Input bindings can be found in the folder `3rdPersonCamera\Settings\Input\TPC_Input`
This provides a default scheme to use or edit.

### Legacy Input system

In addition when using any of the upper components, a corresponding 
input sampling component has to be added on the game object with the CharacterController.

There are 3 input sampling components
- `CameraInputSampling_FreeForm`
	Needed when FreeForm is used
- `CameraInputSampling_LockOn`
	Needed when LockOnTarget is used
- `CameraInputSampling_Shoulder`
	Needed when OverTheShoulder is used

------------------------------
## Basic Setup


Add the following scripts to the main camera gameobject:
- `CameraController`
- `FreeForm`
- `CameraInputSampling_FreeForm` (Legacy Input) or `CameraInputSampling` (New Input System)

The most important option is which Transform to follow. Set this in `Target`.

Then set the collision layer which layers should be picked up by the camera. (Usually default)
> When your target has a collider, which it likely has, it's important to set the `Player Layer`.
The layer of the target has to be a seperate layer (for example "Player" layer) so the 3rd Person Camera 
can differentiate between player and the surrounding world and resolve self-collisions.

The rest can be tweaked in a number of ways:
(Instead of reading this document, the same descriptions can found in each tooltip!)

------------------------------

## CameraController component

### BASIC SETTINGS
- `Target` (Transform)
	- Set this to the transform the camera should follow, can be changed at runtime
- `Desired Distance` (float)
	- The distance how far away the camera should be away from the target
- `Collision Distance` (float)
	- Offset for the camera to not clip in objects
- `Offset vector` (Vector3)
	- change this vector to offset the pivot the camera is rotating around	
- `Camera Offset` vector (Vector3)
	- offset the camera in any axis without changing the pivot point

- `DisableZooming` (bool)
    - Prevents any input to zoom in/out
- `Zoom Out Step Value` (float)
	- The distance how fast the player can zoom out with each mousewheel event
- `Occluded Zoom In Speed` (float)
	- The speed, how fast the camera can zoom in automatically to the desired distance.
- `Unoccluded Zoom Out Speed` (float)
  - The speed, how fast the camera can zoom out automatically to the desired distance.
- `Camera Offset Change Speed` (float)
  - How fast the offset can change.
- `Camera Offset Min Distance` (float)
  - The minimum distance the camera offset can decrease to.
- `Hide Skinned Mesh Renderes` (bool)
	- Automatically turns off a targets skinned mesh renderer when the camera collider hits the player collider
	
### COLLISION LAYER SETTINGS
- `Collision Layer` (LayerMask)
	- Set this layermask to specify with which layers the camera should collide
- `Player Layer` (LayerMask)
	- Set this to your player layer so ground checks will ignore the player collider

### FEATURES
- `Occlusion Check` (bool) 
	- Automatically reposition the camera when the character is blocked by an obstacle.
- `Smart Pivot` (bool) 
	- Uses a pivot when the camera hits the ground and prevents moving the camera closer to the target
	when looking up.
- `Thickness Check` (bool) 
	- Thickness checking can be configured to ignore  smaller obstacles like sticks, 
	trees, etc... to not reposition or zoom in the camera.  
#### SMOOTH TARGET MODE
- `Smooth Target Mode` (bool)
	- Enable to smooth out target following, to hide noisy position changes that should not be picked up by the camera immediately
	or to enable smooth transitions to other targets that are changed during runtime
- `Smoothing Target Value/Speed` (Vector3)
	- The speed at which the camera lerps to the actual target position for each axis. 0 means no smoothing!

### THICKNESS CHECK SETTINGS
- `Max Thickness` (float) 
	- Adjusts the thickness check. The higher, the thicker the objects can be and there will be no
	occlusion check. Warning: Very high values could make Occlusion checking obsolete and as a result
	 the followed target can be occluded

### SMART PIVOT SETTINGS
- `Smart Pivot only on ground` (bool)
	- Smart pivot is only activated on stable grounds
- `Floor Normal Up` (Vector3)
	- Default ground up for smart pivot
- `Max Floor Dot` (float)
	- Only hit results from maxFloorDot to 1 will be valid

### STATIC PIVOT ROTATION SETTINGS
- `Pivot Rotation Enabled` (bool)
	- Enable pivot rotation feature to rotate the camera in any direction without the camera moving or rotation around. Used in LockOn and Follow.
- `Smooth pivot` (bool)
	- When enabled the pivot will smoothly slerp to the new pivot angles
- `Custom Pivot` (bool) 
	- Enable overriding the pivot with the public variable 'pivotAngles'
- `Pivot Angles` (Vector3)
	- Custom override vector in Euler angles to modify the current pivot rotation

### EXTRA SETTINGS
- `Max Raycast Hit Count` (int)
	- The maximum amount of raycast hits that are available for occlusion hits. Can be tweaked 
	for performance. Don't set amount less than 2 when your geometry has many polygons as this can 
	make repositioning unreliable.
- `Max Thickness Raycast Hit Count` (int)
	- The maximum amount of raycast hits that are available for thickness hits. Can be tweaked for
	performance. Use an even amount to account for back-front face and don't use less than 2.	
	
### Public variables
- bool `playerCollision` (get/set)
   - Set to true to deactivate the player model    
- float `Distance` (get)
  - The current distance from the target plus offsetvector to the camera

### Public methods (needed for runtime changes)
- `UpdateOffsetVector(Vector3)`
	- Updates the offset vector during runtime and fixes vectors which are too small
- `UpdateCameraOffsetVector(Vector3)`
	- Updates the camera offset vector during runtime. Important! -> Direct manipulation of camera offset vector won't update the needed boolean for testing camera offset vector collisions
- `InitFromTarget()` or `InitFromTarget(Transform)`
  - Re-Init a new target during runtime. No parameters uses the current target
  
- `CameraControllerState GetControllerState()`
  - Get all the state values from the camera controller
- `GetCameraPosition(in CameraControllerState, Vector3 targetPosition)`
	Get the unoccluded/desired camera position

------------------------------

## FreeForm component:
This script handles the main rotation mechanic of the camera and is used for Freeform camera movement. It's not needed if you just
want follow mode! It's dependency is the CameraController. It can be extended with the LockOnTarget component or OverTheShoulder component.	

### BASIC SETTINGS 
- `Camera Enabled` (bool) 
	- Enables/Disables the camera rotation updates. Useful when you want to lock the camera in place or
	to turn off the camera, for example, when hovering an interface element with the mouse
- `Rotation Speed` (Vector2)
  - How fast the camera can rotate.
- `Min/Max distance` (float) 
	- Sets a min/max distance for zooming in or out	
- `Min/Max Angle limit` (float) :
	- Sets a limit for the angle how much you can look up or down		

### STATIONARY SETTINGS
- enum `StationaryModeType`:
  - Free: no limits in angle
  - Fixed: can't move axis
  - Limited: limited in angles
  - Rotate when Limited: limited by angles, when over threshold rotate the camera
- `Stationary Mode Horizontal` (StationaryModeType)
	- Sets a stationary mode for the horizontal axis
- `Stationary Mode Vertical` (StationaryModeType)
	- Sets a stationary mode for the vertical axis
- `Stationary Max Angle Horizontal` (float)
	- Maximum angle for the horizontal axis
- `Stationary Max Angle Vertical` (float)
	- Maximum angle for the vertical axis

### EXTRA SETTINGS
- `Smoothing` (bool)
	- Enables smoothing for the camera rotation when looking around - Beware, introduces interpolation lag
- `Smooth speed` (float) 
	- The speed at which the camera will lerp to the final rotation
- `Force character direction` (bool)
	- Sets the targets y-axis to have the same y rotation as the camera.
- `Stabilize rotation` (bool)
	- Sets the z-axis rotation to 0 and stabilizes the rotation. Only use when the z-axis is drifting.
- `Custom Input` (bool)
	- Inform the script of using a custom input script to set the CameraInputFreeForm model
- `Looking backwards enabled` (bool)
	- Enables looking backward with pressing middle mouse button

------------------------------
## Follow component
This script handles following the target without any manual camera input. Useful for games that handle non-humanoid targets 
like racing or flying games. Its dependency is the CameraController.

### BASIC SETTINGS 
- `EnableFollow` (bool) 
	- Enables/Disables the follow mode
- `FollowMode` (enum)
	- TargetForward: Always aligns with the target forward.
    - LookAt: Adventure camera which tries to look at the target and moving as little as possible.
- `Rotation Speed` (float) 
	- How fast the camera should align to the transform.forward of the target
- `Tilt Vector` (Vector3) 
	- Applies an additional vector to the target position to tilt the camera.
- `Disable Time` (float)
	- The default time in seconds the script will be disabled when the player has input in FreeForm
- `EnablePreemptiveSweeping` (bool)
	- Used for LookAt mode. Rotates the camera when target moves around a corner. 
- `SweepSpeed` (float)
  - How fast the camera should align to the target.forward when sweeping.


### SLOPE ALIGNING
- `Align on Slopes` (bool) 
	- Enables/Disables automatic alignment of the camera when the target is moving on upward or downward slopes
- `Rotation speed slopes` (float)
	- The speed at which the camera lerps to the adjusted slope rotation
- `Align direction downwards` (bool) 
	- Set the camera to rotate downwards instead of upwards when the target hits a slope
- `Ground Layer Mask` (LayerMask) 
	- The layer which should be used for the ground/slope checks. Usually just the Default layer.

#### BACKWARD MOTION ALIGNING
- `Look Backwards` (bool)
	- Enables/Disables looking backwards
- `Check Motion for Backwards` (bool) 
	- Enables/Disables automatic checking when the camera should look back
- `Backwards Motion Threshold` (float) T
	- The minimum magnitude of the motion vector when the camera should consider looking back
- `Angle Threshold` (float) 
	- The minimum angle when the camera should consider looking back

------------------------------
	
## LockOnTarget component
This script handles locking onto targets. It's dependencies are the CameraController and Freeform component.

### BASIC SETTINGS
- `Follow Target` (Targetable) 
	- When not null, the camera will align itself to focus the Follow Target.
- `CastRadius` (float)
  - The radius/units around the target in which targets will be acquired.
- `Rotation Speed` (float) 
	- How fast the camera should align to the Follow Target
- `Tilt Vector` (Vector3)	
	- Applies an additional vector to the target position to tilt the camera.
- `Normalize Height` (bool)
	- Ignores the resulting height difference from the direction to the target.
- `Disable Time` (float)
	- The default time in seconds the script will disable 'Smooth Pivot' when the player has input in FreeForm

### TARGETABLES SORTING SETTINGS
- `Default Distance` (float)		
	- A default distance value to handle targetables sorting. Increase when game world and distances are very large.
- `Angle Weight` (float) 
	- A weight value to handle targetables sorting. Increase to prefer angles over distance
- `Distance Weight` (float) 
	- A weight value to handle targetables sorting. Increase to prefer distance over angles		

### EXTRA SETTINGS
- `Forward from target` (bool) 
	- Use the forward vector of the target to find the nearest Targetable. Default is the camera forward vector	
- `Custom Input` (bool)
	- Inform the script of using a custom input method to set the CameraInputLockOn model
- `Max allocated targets`
  - The amount of maximum targets that will be acquired for the sphere overlap cast.

------------------------------

## Targetable component:
Every Target that's focusable needs a Targetable component.	

- `Offset` (Vector3)
	- Set an offset for the target when the transform.position is not fitting.

------------------------------
## DisableFollow component
Disables follow component at adjustable triggers to allow FreeForm. Its triggers are target motion, time and mouse input.

### BASIC SETTINGS
- `activateMotionCheck` (bool) 
	- Enables motion check with its motionThreshold
- `activateTimeCheck`  (bool) 
	- Enables timed checks and resets to follow after timeToActivate seconds are elapsed
- `activateMouseCheck` (bool) 
	- Enables reactivation of follow when mouse buttons are released
- `timeToActivate` (float) 
	- Time in seconds when follow is reactivated. It will also not reactivate when the user still has input.
- `motionThreshold` (float) 
	- Distance which has to be crossed to reactivate follow

------------------------------
## OverTheShoulder component
With this script the camera can change into an aim and release mode by pressing the right mouse button.

### BASIC SETTINGS
- `Aim offset length` (float) 
	- The distance the camera moves away from its zero position. 0.5 means it'll set the max camera offset vector to (axis * maxValue), so -0.5f to 0.5f
- `Aim distance` (float)
  - The distance from the target when aiming
- `Release distance` (float)
  - The distance from the target when released
  
- `Base Offset` (Vector3) 
	- The base offset serves as starting and endpoint when releasing.
- `Slide Axis` (Vector3) 
	-  You can tweak the axis on which the camera slides on, usually it will be just operating on the x axis to slide 
	left and right from the targeted character but it can be changed to any direction in case gravity changes for example.
	The intended design is to use normalized values between -1 and 1
	The difference to the "Additional Axis Movement" vector is that the slide axis goes back and forth when aiming/releasing
	were the additional axis is fixed.
- `Additional Axis Movement` (Vector3) 
	- This axis can be used to have additional offsets when aiming. Unlike the slide axis this
	axis is intended for non-normalized values much like the base offset. 
	It can be used to make the camera zoom high above the character for example when aiming.	

### EXTRA SETTINGS
- `Custom Input` (bool)
	- Inform the script of using a custom input method to set the CameraInputShoulder model

------------------------------
## FreeForm - Camera Input Sampling component (Legacy)
This script handles input sampling and automatically updates the needed input model of FreeForm

### INPUT SETTINGS
- `Camera Mode` (enum)
	- Always and hold - Either the camera rotation is always on or you have to press the mouse button to look around

### MOUSE SETTINGS
- `Mouse Input` (List<int>)
	- A list of integer mouseButton values to enable freelook, default is left/right mouse button
- `Mouse Sensitivity` (Vector2)
	- Adjusts the sensitivity of looking around with the mouse
- `Mouse Invert Y` (bool)
	- Inverts the Y-Axis

### KEYBOARD INPUT SETTINGS
- `Keyboard Input` (List<KeyCode>:
	- A list of KeyCodes to enable freelook

#### CONTROLLER INPUT SETTINGS
- `Controller Enabled` (bool)
	- Enables controller support
- `Gamepad/Button Input` (List<string>:
	- A list of button strings to enable freelook
- `Controller Invert Y` (bool)
	- Inverts the Y-axis
- `Controller Sensitivity` (Vector2)
	- Adjusts the sensitivity of looking around with the controller

### LOCK MOUSE SETTINGS
- `Lock Mouse Cursor` (bool)
	- When looking around the mouse cursor will be locked
- `Keyboard Input Toggle` (KeyCode)
	- A list of KeyCodes to cancel locked mouse cursor
- `Keyboard Input Cancel` (KeyCode)
	-  A list of KeyCodes to cancel locked mouse cursor

### EXTRA SETTINGS
- `Force Direction Feature` (bool)
	- Enable this feature to automatically handle FreeForm.forceCharacterDirection with given inputs
- `Mouse Input Force Direction`
	- A list of integer mouseButton values to enable forcing character/target rotation

Public variables:
- `InputSamplingEnabled` (bool)
	- Hidden in inspector, enables input sampling

------------------------------
## LockOn - Camera Input Sampling component (Legacy)
This script handles input sampling and automatically updates the needed input model of LockOnTarget

### BASIC SETTINGS
- `Cycle cooldown` (float)
	- The time in seconds after which next/previous cycling gets reset to the beginning or 0 index of available Targetables

### INPUT MOUSE SETTINGS
- `Mouse LockOn Input` (List<int>)
	- A list of integer mouseButton values to lock on a target, default is right mouse button
- `Mouse - Cycle target Next` (List<int>)
	- A list of integer mouseButton values to cycle to the next locked on target
- `Mouse - Cycle target Previous` (List<int>)
	- A list of integer mouseButton values to cycle to the previous locked on target

### INPUT KEYBOARD SETTINGS
- `Keyboard LockOn Input` (List<KeyCode>)
  A list of KeyCode values to lock on a target
- `Keyboard - Cycle target Next` (List<KeyCode>)
  A list of KeyCode values to cycle to the next locked on target
- `Keyboard - Cycle target Previous` (List<KeyCode>)
  A list of KeyCode values to cycle to the previous locked on target

### INPUT CONTROLLER SETTINGS
- `Gamepad/Button LockOn Input` (List<string>)
	- A list of button strings to lock on a target
- `Gamepad/Button - Cycle target Next` (List<KeyCode>)
	- A list of button strings to cycle to the next locked on target
- `Gamepad/Button - Cycle target Previous` (List<KeyCode>)
	- A list of button strings to cycle to the previous locked on target

  Public Variables:
	- `InputSamplingEnabled` (bool)
	  Hidden in inspector, enables input sampling

------------------------------
## Shoulder - Camera Input Sampling  component (Legacy)
This script handles input sampling and automatically updates the needed input model of OverTheShoulder

### INPUT SETTINGS
- `Input Mouse Aim Mode` (List<int>) 
	- A list of Mouse button values to activate aiming - Default is right mouse button
- `Input Keyboard Aim Mode` (List<KeyCode>)
	- A list of KeyCode values to activate aiming
- `Input Gamepad Aim Mode` (List<string>) 
	- A list of button strings to activate aiming
- `Input Mouse Change Leaning` (List<int>) 
	- A list of Mouse button values to toggle the side you are leaning to
- `Input Keyboard Change Leaning` (List<KeyCode) 
	- A list of KeyCode values to toggle the side you are leaning to		
- `Input Gamepad Change Leaning` (List<string>) 
	- A list of button strings to toggle the side you are leaning to
- `InputSamplingEnabled` (bool) 
	- Hidden in inspector, enables input sampling

Public Variables:
- Input Sampling Enabled (bool)
	- Enable/Disable input sampling

Additional setup for controller support:
For an easier setup use the preconfigured InputManager.asset from the 3rdPersonCamera/ProjectSettings folder. 
!Caution! - Doing so will overwrite any InputManager data you already have!

If you get this warning: "Controller Error - Right axis not set in InputManager. Controller is disabled!"
you have to set the following axis in the InputManager:
"Right_3": 3rd axis
"Right_4": 4th axis
"Right_5": 5th axis 
0 gravity, 0.3 dead, 1 sensitivity are good standard values.

If you need other names you can change them in the script.

------------------------------

### Controls for demos

- WASD for movement
- Left/right click to rotate in freeform demo
- Right click, lock on target in target demo
- "r" key to reset car/ball
- "q" and "e" to rotate camera in ball demo

------------------------------
## Version History

### 1.1.2
	- Hotfix for 'AlignOnSlopes' feature missing the smart pivot rotation in `TargetForward` mode
### 1.1.1
	- Fixed `AlignOnSlopes` feature to work with `LookAt` follow mode
### 1.1
	- Camera offset vector behaviour has been improved and instead of just switching sides when 
	  it's colliding, it will smoothly realign to the center. Furthermore, it will try to lerp smoothly
	  in almost every situation unless the camera will collide with something.
	- Moved all relevant data to `CameraControllerState` to support better saving/loading.
	  Use `GetControllerState` and `ApplyControllerState`. Keep in mind this only applies to 
	  state and not settings or the current `Target`!
### 1.0.8.1
	- fixed a clamp bug in LockOnTarget
	- renamed "InputManager.asset" to "InputManager.asset.template" to supress the warning
	- removed default code generated file "TPC_Input" from base package
### 1.0.8
    - Improved and rewrote every feature
    - Added "Adventure" camera for "Follow": 
        Use FollowMode "LookAt" to activate
    - Added new Input system bindings:
        Remove the old "CameraInputSampling_xxx" scripts and add the "CameraInputSampling" component.
        Don't forget to change to the new Input System in the Player preferences.
        Control bindings can be found in "3rdPersonCamera\Settings\Input"
    - Split demo scenes into its own .unityPackage
    - Integrated the free KCC (KinematicCharacterController) for better character movement
      and easier out of the box usage  
	- Updated and reformated documentation
### 1.0.7.2
	- The main transform.rotation has been split up in new Quaternions for camera rotation, pivot and smart pivot rotations:
	  Not only is the smart pivot now smoothly integrated in the rest of the systems, the new pivot opens up another level of control. Rotations without the camera moving/rotating around.
	- Raycast hit sorting has been improved which should solve most jitter issues (Haven't found them while testing)
	- Thickness check improvements, more stable results
	- Smart pivot ground check improvement where it would not trigger smart pivot on slopes. There is now also an option to have smart pivot only on grounds and/or walls
	- Repositioning when colliding has been improved and getting back to the the desired distance should be more stable/smooth
	- A new demo scene: car with lock on
	- Introduced new parameters:
		Camera Controller
		- Smart Pivot only on ground
		- Pivot Rotation Enabled
		- Smooth pivot: pivot will smoothly slerp to new pivot
		- Custom Pivot: enable to use pivot angles for override
		- Pivot Angles
		Free Form
		- Stationary Mode Horizontal/Vertical
		- Stationary Modes (enum)
			* Free: no limits in angle
			* Fixed: can't move axis
			* Limited: limited in angles
			* Rotate when Limited: limited by angles, when over threshold rotate the camera
		- Stationary max angle Horizontal/Vertical
		Follow
		- Disable time: The default time in seconds the script will be disabled when the player has input in FreeForm
		LockOn
		- Disable time: When there is input in freeform, LockOn will disable the "smooth pivot" mode and enable it after the disable time 
### 1.0.6.3
	- fixed regression in input sampling where camera mode set to Always was not working
	- added "Ignore X Rotation" to Follow so the player can look up/down in follow mode
### 1.0.6.2
	- cleanup of prefabs
### 1.0.6.1
	- added MouseCursorHelper for Windows platform to support cursor locking in place
	  and not in the center of the screen
	- added gamepad button sampling to FreeForm input sampling
	- added public bool inputSamplingEnabled to all input sampling scripts
	  with which sampling can be disabled (for example when hovering interface elements)
	- added forceDirectionFeature to FreeForm input sampling, which handles
	  forcing the direction of FreeForm with mouse buttons (for example, only force direction with right mouse button)
	- added stabilizeRotation to FreeForm in case the z-rotation starts to drift
### 1.0.6.0
	- added TPC_DEBUG to CameraController which enables debugging options for raycasts visualization.
	  Either uncomment TPC_DEBUG in the script directly or define in Unity project settings
	- improved unreliable raycast hit handling on edges and sorting for the best results
	- improved distance handling on repositions

### 1.0.5.9
	- Fixed a glitch for 0/0/0 offset vectors where the pivot point of the target is inside the collision sphere.
	  The offset gets fixed in Awake of the CameraController, during runtime please use UpdateOffsetVector(Vector3) to prevent glitches.
### 1.0.5.8
	- Input sampling of FreeForm, OverTheShoulder and LockOn has been split into seperate scripts
	  and a data oriented model is now used for main scripts:
	  * FreeForm uses struct CameraInputFreeForm
	  * LockOnTarget uses struct CameraInputLockOn
	  * OverTheShoulder uses struct CameraInputShoulder
	  
	  These scripts are updated with the UpdateInput(CameraInput...) method. This method should be used for 
	  custom input sampling scripts

	- added CameraInputSampling_X components which samples inputs and automatically updates 
	  the according input model with UpdateInput(CameraInput...)
	  These scripts can be used as a template for custom input sampling or adding additional inputs
	- added component CameraInputSampling_FreeForm
	- added component CameraInputSampling_Shoulder
	- added component CameraInputSampling_LockOn	
### 1.0.5.7
	- fixed occurances where automatic zooming out failed and the camera would reposition itself too fast
	- improved smart pivot transition
	- improved camera offset clipping
### 1.0.5.6
	- added headers and tooltips to components and fields to provide easier documentation
	- changed smoothTargetValue in CameraController from float to Vector3 to have more control over axis 
### 1.0.5.5
	- added a stabilization for follow/lock on rotations to prevent wrong rotations in Z-axis
	- added mouse/keyboard/gamepad input to LockOnTarget for locking on target and switching to next/previous
	- improved lock on target mode with weighted distance and angle
	- added input properties for aiming and switching sides to OverTheShoulder
	- added "forwardFromTarget" to LockOnTarget which provides a more stable sampling of targets when cycling through targets is preferred
	  the default mode takes the forward from camera - which gives the best results when cycling isn't needed and the target that is looked at is more important
### 1.0.5.4
	- added forceCharacterDirection to FreeForm so the character turns automatically in the same direction the camera is facing.
	  This works only for character controllers that use absolute directions for movement. The demo character controller
	  uses relative movement, so it won't work right.
	- fixed a sorting bug with occlusion hits
	- improved thickness logic for clipping geometry that occurs often in prefab based level design
### 1.0.5.3
	- improved jitter when camera was zooming out against a wall
	- improved thickness handling
### 1.0.5.2
	- added min/max distance to CameraController
	- added namespace for demoscene scripts
### 1.0.5
	- added an additional camera offset vector for skewed over-the-shoulder camera type of games like "The Division"
	  that can either be fixed or controlled via the OverTheShoulder script
	- added OverTheShoulder component
	- added new scene to demonstrate Camera Offset called "DivisionCamera - DemoScene"
	- changed PlayerLayer data type from integer to LayerMask for convenience and the possibility to have more player layers
	- added "Smooth Target Mode" and "Smooth Target Value" that lerps to the target position for smoother translations/rotations
	  to dampen noisy position changes for either physics or network based games	
	- added support for having no target when initializing or starting
	- added mouse/keyboard inputs to FreeForm which uses a list of mouse buttons (Integer) and keyboard inputs (KeyCode)
### 1.0.4
	- added smart DisableFollow script to utilize FreeForm + Follow
	- added Ball demo scene
	- added simple BallController script
	- added namespace "ThirdPersonCamera" to scripts
	- added more camera prefabs (Basic, Follow, FreeForm and Ultimate (Freeform/Follow/DisableFollow))
	- improved smart pivot transition from sloped surfaces
	- improved smoothness of alignToSlopes feature
	- improved camera when offset clips into geometry
	- restored Ethan crouch animation
	- removed Ethan air->ground crouch animation
### 1.0.3
	- added 2 new demo scenes (follow and follow+lock on)
	- added support for follow mode
	- added support for target locking
	- split CameraController into CameraController and Freeform	
	- improved smart pivot and occurances of snapping
	- improved thickness check	
	- changed hardcoded raycast layermasks to be configured in the editor
### 1.0.2	
	- changed collision sensitivity to be spherical
	- removed camera position/rotation initialization, editor values are now taken
	- improved smart pivoting and smart pivoting start and resets
	- improved detection algorithm when multiple raycasts are hit
	- added terrain to demoscene
	- added RotateTo public method
	- added x/y, playerCollision, Distance public get or/and set
### 1.0.1
	- improved thickness checking
	- improved smart pivoting on slopes
	- added collision distance to improve clipping occurrences	
	- added automatic mouse cursor locking when looking around (currently unstable in WebGL)
	- added interface handling to demo scene	
	- changed sensitivity handling
	- removed unnecessary files
	- added a script only package
### 1.0.0
	- initial release
