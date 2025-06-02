# Punch_It
SphereChase-Punch is a Unity ML-Agents environment where a learning agent, locate a randomly placed sphere, and physically interact with it using a controlled punch or collision. The objective is to train the agent to approach the target efficiently and apply force from the correct range and angle.
![image](https://github.com/user-attachments/assets/062dde7c-7ab0-45ab-8069-75e51ac0c742)



# 🥊 Punch_It: Mohamed Ali's Agent Smackdown
## 🎮 Project Overview

**Punch_It** is a physics-driven Unity ML-Agents environment where a humanoid agent learns to locate a randomly moving target (a vertically bobbing metallic sphere) and deliver a perfectly-timed punch via animation. The goal? Maximize contact quality while minimizing idle flailing or failed swipes.

This serves as a minimal and efficient sandbox for:
- Discrete action space experimentation 🧠
- Movement + target acquisition training 🎯
- Cooldown-regulated punch timing ⏱️
- Reward shaping debugging ✅

---
# Key features include:

🔄 Randomized target placement on each episode reset, while the sphere constantly mobes up adn down 

🧍‍♂️ Physics-based humanoid punch animation

## 🤖 ML-Agent Task: "PunchBot Precision"

This task trains a humanoid ML-Agent to walk, rotate, and time its punches to hit a vertically oscillating target (sphere). The agent operates with **3 discrete branches**:
## 🔁 Agent Brain Setup (Discrete Action Space)

| Branch | Function      | Values                      | Description                          |
|--------|---------------|-----------------------------|--------------------------------------|
| 0      | Walk          | 0 = Idle, 1 = Walk          | Forward movement toggle              |
| 1      | Turn          | 0 = Left, 1 = Right, 2 = Stay| Rotational movement                  |
| 2      | Punch Trigger | 0 = No punch, 1 = Punch     | Animator trigger `"Punch"`          |

---
## 👁 Observations (3 Vector Inputs)

1. **Distance to sphere** (normalized float)
2. **Angle between agent forward and sphere** (normalized float)
3. **Cooldown timer** (normalized float between 0 and 1)


👊 Reward signal for successful contact with the sphere
| Behavior              | Reward                        | Notes                                  |
|-----------------------|-------------------------------|----------------------------------------|
| Walks toward sphere   | `+0.5 * (1 - normDistance)`    | Encourages pursuit                     |
| Faces sphere          | `+0.5 * (1 - normAngle)`       | Rewards orientation alignment          |
| Punch success         | `+10f`                         | Main goal achieved                     |
| Punch miss/cooldown   | `-0.1f`                        | Discourages spam                       |
| Agent falls off       | `-10f`                         | Harsh penalty                          |
| Idle too long         | `-0.02f` (optional)            | Light discouragement                   |

> ⚖️ *Reward shaping was tuned for PPO + LSTM to balance exploration and precision.*

⛔ Penalties for missing, standing idle, or leaving bounds
---
### 💡 Training Setup:
🧠 Custom observations: distance to target and orientation
### 🎯 Task Objective:
Approach and precisely punch a sphere that moves up and down on a fixed plane. The environment evaluates:
- Distance to the target (float)
- Angle between agent’s forward vector and target (float)
- A normalized cooldown timer to restrict repeated punches (float)
  

- **Observations:** 3 floats (distance, angle, punch cooldown)
- **Max Steps per Episode:** 10,000
- **Rewards:**
  - `+1` for a successful punch
  - `+0.5` for inverse angle to represent closeness
  - `-0.5` for a punch during cooldown or a miss later -0.1 for incorrect 
  - `-1` if the agent falls off the terrain (`transform.y < -1`)


🧭 Quick Reward Debug Checklist:
| Behavior              | Reward                     | Comment                                  |
| --------------------- | -------------------------- | ---------------------------------------- |
| Walks toward sphere   | +0.5 \* (1 - normDistance) | ✅ Good shaping                           |
| Faces sphere          | +0.5 \* (1 - normAngle)    | ✅ Good shaping                           |
| Idle too long         | -0.02f                     | ❓ May discourage strategic pauses// removed         |
| Punch wrong           | -0.02f                     | ✅ Light penalty, good                    |
| Punch during cooldown | -0.1f                      | ⚠️ Harsh, and may discourage exploration// removed |
| Punch success         | +10f                       | ✅ Clear strong signal                    |
| Falls off plane       | -10f                       | ✅ Harsh failure signal                   |


# Do You Need Punch Penalties if You Already Have Inverse Rewards?
You're right to be thinking in terms of redundancy , however:

- Inverse rewards guide "what to do" (get close, face target).

- Punishments guide "what NOT to do" (punch too early, spam attacks, etc).

  
#  My Steps

### 🌌 1. Visuals & Setup

- Space-themed skybox ☁️
- Metallic blue oscillating sphere 🔵
- Agent with humanoid model + animator
- Ground plane + particle effects ✨
   ![image](https://github.com/user-attachments/assets/c9b37ded-4ec2-4c0b-a41f-8f9afc2b1fe8)

2.### 🕴️ 2. Character & Animation Logic

- **Imported humanoid model**
- **States**: Idle, Walk, Left, Right, Punch
- **Animator Parameters**:
  - `Idle` (bool)
  - `Left` / `Right` / `Walk` (bools)
  - `Punch` (trigger) 
![image](https://github.com/user-attachments/assets/d0a38caa-8c28-471a-ad11-f6b6de5d95bd)

3. downloaded animations no skin
   ## 🎬 Animation: Punch
### States:
- **Idle**
- **Left** (Triggered when `Left` = true, back to Idle when `Idle` = true)
- **Right** (Triggered when `Right` = true, back to Idle when `Idle` = true)
- **Walk** (Transition from Right or Left when `Walk` = true, back to Left or Right when `Left` or `Right` = true)
- **Punch** (to Idle when `Idle` = true, to Walk if `Walk` = true, or back to Idle if `Punch`triggred )

### Transitions:
- Add parameters: `Idle` (bool), `Left` (bool), `Right` (bool), `Walk` (bool), `Punch` (trigger)
- Setup transitions in Animator accordingly.

  ![image](https://github.com/user-attachments/assets/f35cee47-b506-46fb-b311-31a7ddd4c7ed)

### 🔁 3. Sphere Behavior

- Sphere **randomly spawns** in 1 of 12 positions
- Oscillates up/down in a sine-wave pattern
- Reset every episode

### 📜 4. Scripts and Punch Logic

- Punch is triggered via Animator `"Punch"` parameter
- Punch cooldown is tracked via a timer
- Cooldown is normalized and passed to Agent observations

#### Reward Issues Debugged:
- Raw distance reward exploded near zero → Normalized to avoid large divisions
- Punch spam punished unless cooldown expired
- Falling = hard episode reset
 - reward dilmea
       ![image](https://github.com/user-attachments/assets/3a5ed2f2-d13e-41eb-b8c6-6880990d7b16)
   - if the distance is small , high value due to lare divdant explosion
   - must be fixed by normlization and to keep bounded
![image](https://github.com/user-attachments/assets/2d64b620-771d-4aa0-a045-5bb876535f69)
      This way:
        - Closer = higher reward (max 0.1)
        - Far = near zero reward
Final code after train tweeaks     
   ![image](https://github.com/user-attachments/assets/fe951d07-0ac0-449d-86f5-4203c084ca38)

5. Mlagent
   
## ⚙️ Training Setup

- **Algorithm**: PPO
- **Memory**: LSTM enabled (for temporal punch timing)
- **Max Steps per Episode**: 10,000
- **Environment Reset**: On fall or timeout
  


https://github.com/user-attachments/assets/d6554650-038c-4c31-90e0-3c09a8eb67d0


![WhatsApp Image 2025-06-02 at 09 34 21_e1b4c11d](https://github.com/user-attachments/assets/bc8fc0b9-e612-4792-8c57-3739fbfbdfb0)



## 5. Scripts

### 🎯 `MlA_Movement.cs` – ML-Agent Brain for Punching Sphere  
**Purpose:**  
Controls an ML-Agents Agent that learns how to walk, rotate, and punch a floating sphere using reinforcement learning and animation logic.

**Core Features:**  
- 🔄 Randomized resets for both the agent and sphere at each episode  
- 🎮 Movement, rotation, and punch logic linked to `DiscreteActions[0-2]`  
- 🧠 Observations include distance to target, angle, and cooldown state  
- 🥊 Reward structure based on proximity, angle, correct punch timing, and episode success  
- 🧰 Integrated with `Animator` to trigger walk, idle, turn, and punch animations  
- ⚠️ Collision detection includes robust null-checks and animation state logic  

---

### 🧪 `AnimatorTestController.cs` – Manual Animation Debug & Testing  
**Purpose:**  
Allows manual testing of movement, rotation, and punch logic via keyboard input before agent training is applied.

**Core Features:**  
- ✅ Walk (`W`), rotate (`A/D`), punch (`Space`)  
- 🧠 Logic mirrors agent controller with simplified hardcoded logic  
- ⏲️ Cooldown system included to simulate learning constraints  
- 🎬 Triggers animation states and handles fallback resets  
- ⚠️ Falls below threshold result in loss material (visual feedback)  

**Used For:**  
- ✅ Verifying animation blend tree transitions  
- ✅ Testing punch hitboxes and angle logic  
- ✅ Debugging force reactions on the sphere  
- ✅ Validating cooldown visuals and material feedback  

---

### 🌐 `Sphere_Bahaviour.cs` – Visual Floating/Bounce Effect for Sphere  
**Purpose:**  
Adds a smooth, floating bounce animation to the magical sphere using a sine wave. This enhances visual feedback and keeps the sphere feeling "alive" while waiting for interaction.

**Core Features:**  
- 🔁 Applies vertical oscillation based on sine wave logic (`Mathf.Sin`)  
- 📌 Remembers the initial position and offsets upward every frame  
- ⚙️ Two public parameters:  
  - `amplitude`: How high it bounces (default: `1f`)  
  - `frequency`: How fast it bounces (default: `1f`)  
- 🧠 Lightweight and performance-friendly — no physics or animations required  
- 🎨 Great for adding subtle motion and making targets feel more dynamic  

**Used For:**  
- ✅ Making the target sphere more visually responsive in idle states  
- ✅ Testing and tuning motion without relying on physics systems  
- ✅ Giving an organic, magical "float" feel to your collectable or target object  
s


## 6. BUGSS 

## 🐞 Bugs & Fixes

| Bug | Fix |
|-----|-----|
| YAML path errors | Double-check paths, re-generated configs |
| Character falls through floor | Removed `CharacterController`, added `Rigidbody` + capsule collider |
| Floating character | Adjusted local `Vector3` transform offset |
| Broken animation | Replaced mutant model with Jones; ensured T-pose and avatar mapping |
| Sphere not moving | Created custom spawner with 12 position pool |
| Instant fail on start | Disabled "lose material" logic from starting state |


## 🧪 Future Extensions (Ideas)
-ADD Learning Strategy (Planned Curriculum):
| Phase | Description                          |
| ----- | ------------------------------------ |
| 1️⃣   | Fixed target, no cooldown            |
| 2️⃣   | Randomized target, still no cooldown |
| 3️⃣   | Add cooldown logic                   |
| 4️⃣   | Add animation punch constraints      |

- Use normal animation instead of script on sphere
- 
## ✅ Summary

Punch_It is a focused ML-agent playground to test:
- Multi-branch discrete action coordination
- Cooldown-regulated attack timing
- Simple reward shaping logic
- Physics & animation integration

Whether you're trying to teach your agent martial arts or just enjoy watching it flail at a space orb, this is a fun and compact learning lab. 🛰️

---
