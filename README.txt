PPO-Unity-Sandbox
=================
A sandbox for building reinforcement learning experiments in Unity.
Inspired by b2studios (https://www.youtube.com/@b2studios).

Implements PPO entirely in C# with GPU-accelerated matrix math.
No Python. No ML-Agents. No external dependencies.


REQUIREMENTS
------------
- Unity 6 (6000.0.x) — install via Unity Hub
- A GPU (required for compute-shader matrix multiplication)


QUICK START
-----------
1. Open the PPO/ folder as a Unity project in Unity Hub
2. Let Unity resolve packages on first open (accept URP upgrade if prompted)
3. Open or create a scene, assign a RunEnvironment + Bootstrap + PhysicsManager
4. Press Spacebar to start


BOOTSTRAP (Inspector)
---------------------
TESTING       true  = watch a saved network run
              false = train from scratch (or from a checkpoint)

LOAD_ID       name of the network to load from the Networks folder
              leave blank to start fresh training

RUN_ID        name used when saving new checkpoints

FOLDER_PATH   leave blank — defaults to Assets/StreamingAssets/Networks/

CHECKPOINT_INTERVAL   how many steps between automatic saves (default 65536)


PHYSICS MANAGER (Inspector)
----------------------------
MODE          REALTIME = run at normal speed
              FAST     = run STEPS physics frames per Unity frame (faster training)

ENVIRONMENTS  list of active Environment components in the scene


NETWORKS
--------
Trained networks are saved to Assets/StreamingAssets/Networks/ as 4 text files:
  {name}_mu.txt       actor mean weights
  {name}_sigma.txt    actor std dev weights
  {name}_value.txt    critic weights
  {name}_running.txt  state normalisation statistics

Network files are gitignored by default. Commit them manually when you want
to ship a demo.


CREATING A NEW EXPERIMENT
--------------------------
See CLAUDE.md for the full guide and the 100m Sprint setup walkthrough.
Short version:
1. Create a new scene
2. Build your agent as a hierarchy of Rigidbodies + Joints
3. Write an Environment subclass (see RunEnvironment.cs as the template)
4. Wire everything up in the Inspector
5. Set TESTING=false and hit Play
