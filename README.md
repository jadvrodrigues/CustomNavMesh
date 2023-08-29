# Custom Nav Mesh

Alternative to Unity's NavMesh system where the **agents avoid the other non-moving agents** in their pathing. It uses the official navigation system, but you have to use its components instead. Compatible with **NavMeshComponents**.

## How it works

**How do the agents avoid others**? Unity's **NavMeshAgent** only goes around **NavMeshObstacles** and ignores the other agents in their path. By disabling **NavMeshAgent** and enabling **NavMeshObstacle** when an agent is not moving noticeably, the agents now avoid other non-moving agents in their pathing.

Unity's NavMeshAgent             |  CustomNavMeshAgent
:-------------------------:|:-------------------------:
![](Assets/Examples/GIFs/1_before.gif)  |  ![](Assets/Examples/GIFs/1_after.gif)

> **1. Setting the destination** to the blue target's **exact** center position.

**What if a moving agent collides against a stopped agent that's currently in obstacle mode**? By default, it wouldn't be able to push it. However, by:

- duplicating the baked surface, obstacle, and agents;
- making the duplicated agents switch between **NavMeshAgent** and **NavMeshObstacle** instead of the original ones;
- having each original agent copy the duplicated one's velocity, and the duplicated one copy the original's position at every frame.

![](Assets/Examples/GIFs/2_navigation_view.gif)

> **2.** The duplicated **NavMesh** components. The **hidden** components are on the **right** side.

**You can** have agents **push** the **other agents**. For the **CustomNavMesh** system to work, you must use its **custom components**, which are **identical to the original ones**, making the transition seamless.

NavMeshAgent vs NavMeshObstacle "Agents"            |  CustomNavMeshAgents
:-------------------------:|:-------------------------:
![](Assets/Examples/GIFs/3_before.gif)  |  ![](Assets/Examples/GIFs/3_after.gif)

> **3. Overlapping** agents by throwing one against the others. The **agents** are green and the **obstacles** blue.

**Why not** just **switch** the agents back to **NavMeshAgent** instead? That wouldn't work because switching from **NavMeshObstacle** to **NavMeshAgent** isn't instant — it would take at least two frames.

**What are the disadvantages?** Every **NavMesh** component is duplicated, which makes it less performant. However, it shouldn't be noticeable in most situations.

## Custom classes

**CustomNavMesh** – you can choose the hidden game objects **relative position** and whether or not they are **rendered** by accessing the class or through its **singleton** present in the **scene**.

![](Assets/Examples/GIFs/4_custom_nav_mesh_fields.gif)

> **4.** Changing **CustomNavMesh** fields.

- **CustomNavMeshSurface** – add to each surface that is going to be baked. These **surfaces** need to be **rendered meshes**. Both **Physics Colliders** and **Terrains** are ignored by it;
- **CustomNavMeshObstacle** – replacement for **NavMeshObstacle**;
- **CustomNavMeshAgent** – replacement for **NavMeshAgent**. The main extra properties are:
  - **Block after Duration** – should the hidden agent start blocking (switch from agent to obstacle) if it hasn't surpassed a certain speed for a given time?
  - **Unblock after Duration** - should the hidden agent periodically try to find a new shorter path and unblock (switch from obstacle to agent) when it finds one?
  - **Unblock at Speed** - should the hidden agent unblock if it surpasses a certain speed?

## How To Get Started

**Clone or download** this repository and open the project in Unity. Alternatively, you can **copy the contents** of `Assets/CustomNavMesh` to an **existing project**.

The **scenes** used for the **GIFs** are available in the `Assets/Examples` folder. Just hit the **Space Bar** in play mode to test it.

Note: This project is using the **Unity 2022.3 LTS** version. Tested in **PC Standalone**, **Android**, and **WebGL**.

## Final thoughts

Things that I might implement later:
- Every time an agent in block mode does a refresh (tries to see if it should unblock), **SamplePosition** and **CalculatePath** are called. Both of these functions are **synchronous** and run on the **main thread**. I could implement the new experimental **NavMeshQuery operations** (not yet fully complete) which can be executed using the new **job system**.
- Import more methods and properties from the original **NavMesh** components.
- Create custom versions of the **NavMeshComponents'** components.

If you **need help** adding this to your project or have any **questions**, just [message me](https://forum.unity.com/members/jadvrodrigues.4503760/) and I'll try to help. I'm also interested in any **tips** or **suggestions** you may have, cheers!



