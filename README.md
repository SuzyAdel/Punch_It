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
- 
- Vector Observation: 4 (or more depending on added observations)
Actions:
  - Branch 0: 2 (Idle, Walk)
  - Branch 1: 3 (Left, Right, No Turn)
  - Branch 2: 2 (Punch or not)


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
  - `+0.5` for inverse angle to represent closeness
  - `-0.5` for a punch during cooldown or a miss 
  - `-1` if the agent falls off the terrain (`transform.y < -1`)
- **Punch Logic:** Animator trigger `"Punch"` controlled by cooldown
- **Network:** PPO with LSTM memory for temporal context and punch timing

# Do You Need Punch Penalties if You Already Have Inverse Rewards?
You're right to be thinking in terms of redundancy , however:

- Inverse rewards guide "what to do" (get close, face target).

- Punishments guide "what NOT to do" (punch too early, spam attacks, etc).

  
#  My Steps

1. created a plane , metalic blue sphere , and added a space skybox
   ![image](https://github.com/user-attachments/assets/c9b37ded-4ec2-4c0b-a41f-8f9afc2b1fe8)

2. added the character , extrracted the materials and creared a star particle with greidoent color and glow
![image](https://github.com/user-attachments/assets/d0a38caa-8c28-471a-ad11-f6b6de5d95bd)

3. downloaded animations no skin
   ## 🎬 Animation: Punch
### States:
- **Idle**
- **Left** (Triggered when `Left` = true, back to Idle when `Idle` = true)
- **Right** (Triggered when `Right` = true, back to Idle when `Idle` = true)
- **Walk** (Transition from Right or Left when `Walk` = true, back to Left or Right when `Left` or `Right` = true)
- **Punch** (From Idle when `Idle` = true, to Walk if `Walk` = true, or back to Idle if `Idle` = true)

### Transitions:
- Add parameters: `Idle` (bool), `Left` (bool), `Right` (bool), `Walk` (bool), `Punch` (trigger)
- Setup transitions in Animator accordingly.

  ![image](https://github.com/user-attachments/assets/f35cee47-b506-46fb-b311-31a7ddd4c7ed)


4. Code
 - reward dilmea
       ![image](https://github.com/user-attachments/assets/3a5ed2f2-d13e-41eb-b8c6-6880990d7b16)
   - if the distance is small , high valu due to lare divdant explosion
   - must be fixed by normlization and to keep bounded
![image](https://github.com/user-attachments/assets/2d64b620-771d-4aa0-a045-5bb876535f69)
      This way:
        - Closer = higher reward (max 0.1)
        - Far = near zero reward
     







   
5. Mlagent
   
## 👁 3. Observations (Agent)
- Distance between character and sphere
- Angle between forward direction and sphere
- Punch cooldown timer (normalized)

Total: **3 vector observations**

 ## 🎮 4. Actions (Discrete)
- Branch 0: Walk forward (2 values: 0 = idle, 1 = walk)
- Branch 1: Turn (3 values: 0 = left, 1 = right)
- Branch 2: Punch (1 values: 0 = trigger punch)


