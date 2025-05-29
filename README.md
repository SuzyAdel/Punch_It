# Punch_It
SphereChase-Punch is a Unity ML-Agents environment where a learning agent, locate a randomly placed sphere, and physically interact with it using a controlled punch or collision. The objective is to train the agent to approach the target efficiently and apply force from the correct range and angle.

# Key features include:

🔄 Randomized target placement on each episode reset, while the sphere constantly mobes up adn down 

🧍‍♂️ Physics-based humanoid punch animation

## 🤖 ML-Agent Task: "PunchBot Precision"

This task trains a humanoid ML-Agent to walk, rotate, and time its punches to hit a vertically oscillating target (sphere). The agent operates with **3 discrete branches**:
- **Branch 0:** Walk forward
- **Branch 1:** Turn left or right
- **Branch 2:** Trigger punch (via animation)

👊 Reward signal for successful contact with the sphere

⛔ Penalties for missing, standing idle, or leaving bounds

🧠 Custom observations: distance to target and orientation
### 🎯 Task Objective:
Approach and precisely punch a sphere that moves up and down on a fixed plane. The environment evaluates:
- Distance to the target (float)
- Angle between agent’s forward vector and target (float)
- A normalized cooldown timer to restrict repeated punches (float)
  
//🧪 A minimal testbed for movement, targeting, and physical interaction learning

### 💡 Training Setup:
- **Observations:** 3 floats (distance, angle, punch cooldown)
- **Max Steps per Episode:** 10,000
- **Rewards:**
  - `+1` for a successful punch
  - `-0.5` for a punch during cooldown or a miss
  - `-1` if the agent falls off the terrain (`transform.y < -1`)
- **Punch Logic:** Animator trigger `"Punch"` controlled by cooldown
- **Network:** PPO with LSTM memory for temporal context and punch timing

This environment is ideal for exploring basic navigation, target acquisition, and reinforcement learning-driven interactions in Unity using ML-Agents.


# Steps

1. created a plane , metalic blue sphere , and added a space skybox
   ![image](https://github.com/user-attachments/assets/c9b37ded-4ec2-4c0b-a41f-8f9afc2b1fe8)

2. added the character , extrracted the materials and creared a star particle with greidoent color and glow
![image](https://github.com/user-attachments/assets/d0a38caa-8c28-471a-ad11-f6b6de5d95bd)

3. downloaded animations no skin
   ## 🎬 Animation: Punch
The punch is triggered using the animation parameter: `Animator.SetTrigger("Punch")`. To replicate or duplicate this animation:
1. Open the Animator Controller.
2. Duplicate the punch state if variations are needed (e.g., PunchLeft, PunchRight).
3. Ensure transitions lead to/from "Idle" and "Walk" smoothly.
4. Keep transitions short and blend times tight to avoid sluggish combat behavior.
  ![image](https://github.com/user-attachments/assets/f35cee47-b506-46fb-b311-31a7ddd4c7ed)


5. 
