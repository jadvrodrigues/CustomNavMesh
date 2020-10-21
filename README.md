# Custom Nav Mesh

Alternative to Unity's NavMesh system where the agents avoid each other. This still uses the official navigation system but you have to use it's components instead. Compatible with NavMeshComponents.

# Custom components

The new components are a replacement for the built-in ones. Here we introduce three components for the navigation system:

* __CustomNavMeshSurface__
* __CustomNavMeshObstacle__
* __CustomNavMeshAgent__

Avoid mixing the original components with these in your project.

# Settings

All the new components secretly spawn an hidden duplicate version of themselves, where the agents on that hidden surface can be toggled between NavMeshAgent and NavMeshObstacle. This allows the agents that are shown to avoid each other, using the hidden ones pathing to move.

You can adjust the hidden settings through script by accessing the __CustomNavMesh__ class, or through the scriptable object __Settings.asset__ located at the `Assets/CustomNavMesh/Resources` folder.

# How To Get Started

Clone or download this repository and open the project in Unity.
Alternatively, you can copy the contents of `Assets/CustomNavMesh` to an existing project.

Additional examples are available in the `Assets/Examples` folder.

Note: This project was created using Unity 2019.4 LTS version.