# Custom Nav Mesh

Alternative to Unity's NavMesh system where the __agents avoid the other non moving agents__ in their pathing. This still uses the official navigation system but you have to use it's components instead. Compatible with NavMeshComponents.

# How it works

__How do the agents avoid others?__ The Unity's __NavMeshAgent__ only goes around __NavMeshObstacles__ and ignores the other agents in their path. By disabling __NavMeshAgent__ and enabling __NavMeshObstacle__ when an agent is not moving noticeably, the agents now avoid other non moving agents in their pathing.

<p float="left">
  <img src="Assets/GIFs/1_before.gif" width="300"/>
  <img src="Assets/GIFs/1_after.gif" width="300"/> 
  <p>Set destination to blue target. <b>Unity's NavMeshAgent</b> on the left, and <b>CustomNavMeshAgent</b> on the right.</p>
</p>

__What if a moving agent collides against a stopped agent that's currently in obstacle mode?__ Normally, it wouldn't push the agent in the obstacle mode. However, if you:
* duplicate the baked surface, obstacle and agents
* make the duplicated agents switch between __NavMeshAgent__ and __NavMeshObstacle__ instead of the original ones
* have the original agent copy the duplicated one's velocity, and the duplicated one copy the original's position at every frame

You can have them push the others. That's what this __CustomNavMesh__ system does "under the hood", you just have to use it's custom components, which are identical to the original ones, making the transition seamless.

(GIF: 2_push_stopped_agents) Left:before Right:after

Instead of doing this, why not just switch the obstacle to agent? That wouldn't work because that switch isn't instant, it takes at least two frames.

__What are the disadvantages?__ Well, every nav mesh component is duplicated, which makes it less performant. Shouldn't make a different unless you have a lot of agents. 

# Custom classes

__CustomNavMesh__ – you can choose the hidden game objects __relative position__ and whether or not they are __rendered__ by accessing the __class__ or through it's __singleton__ present in the __scene__.

(GIF: manipulating the CustomNavMesh fields)

* __CustomNavMeshSurface__ – add to each surface that is going to be baked. The __surfaces__ need to be __rendered meshes__ and not __Physics Colliders__ or __Terrains__.
* __CustomNavMeshObstacle__ – replacement for __NavMeshObstacle__.
* __CustomNavMeshAgent__ – replacement for __NavMeshObstacle__. It has some added properties:
  * __Time to Block__ – time in __seconds needed__ for the agent __to start blocking__ (to make others go around it),  assuming it hasn't surpassed the __Unblock Speed Treshold__ during the interval.
  * __Unblock Speed Threshold__ – speed at which the agent __stops blocking__ and moves instead.
  * __Block Refresh Interval__ – time in __seconds needed__ for the agent to __check__ if it should __stop blocking__.
  * __Min Distance Boost to Stop Block__ – When block is __refreshed__, this is the __minimum distance__ the newly calculated reacheable position must be __closer__ for the agent to __stop blocking__.
  * The rest have to do with the blocking obstacle carving and are identical to the __NavMeshObstacle__ carving settings.

# How To Get Started

Clone or download this repository and open the project in Unity.
Alternatively, you can copy the contents of `Assets/CustomNavMesh` to an existing project.

Note: This project was created using __Unity 2019.4 LTS__ version. Tested in __PC Standalone__, __Android__ and __WebGL__.

# To be implemented

* Every time a agent in block mode does a refresh (tries to see if it should unblock), __SamplePosition__ and __CalculatePath__ are called. Both of these functions are __synchronous__ and run on the __main thread__. I could divide them by the frames or make them run in separate threads to avoid ms spikes.
* Import more methods and properties from the __NavMesh__ and __NavMeshAgent__ to the __CustomNavMesh__ and the __CustomNavMeshAgent__, respectively. 
* Create custom versions for the __NavMeshComponents__' components.

Until now, I have only added the things I need for another project, but that should be enough for most cases. If you __need help__ implementing some of these or have any __question__, just __message me__ and I'll try to help. I'm also interested in any __tips__ or __suggestions__ you may have, cheers!
