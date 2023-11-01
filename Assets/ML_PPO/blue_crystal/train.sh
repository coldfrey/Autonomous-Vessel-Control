#!/bin/bash
#SBATCH --account=COSC027924
#SBATCH --job-name=first_job
#SBATCH --partition=test
#SBATCH --nodes=1
#SBATCH --ntasks-per-node=1
#SBATCH --cpus-per-task=28
#SBATCH --time=0:59:00
#SBATCH --mem=5000M


# 1. Create and activate a Python virtual environment.
python3 -m venv myenv
source myenv/bin/activate
pip install --upgrade pip
pip install pydrive

# 2. Install the ML Agents toolkit.
python -c "
try:
    import mlagents
    print('ml-agents already installed')
except ImportError:
    import os
    os.system('pip install mlagents==0.27.0')
    print('Installed ml-agents')
"

# 3. Download the config file and Unity executable from Google Drive.

# Direct download links converted from the shared URLs
yaml_link="https://drive.google.com/uc?export=download&id=12hXL2oNMrQE9hX3fg5O9H7bU8KZkuFI7"
creds_link="https://drive.google.com/uc?export=download&id=1X-UzOz-NO7U8P_m0qXU3fAfHbgLomIyc"
setting_link="https://drive.google.com/uc?export=download&id=1DnGWbR_w-MVa3KsxsGaJ_OcvgAavKut3"

# navigate to the directory where the files will be downloaded ~/ML_PPO
mkdir ML_PPO
cd ~/ML_PPO

wget -O trainer_config.yaml $yaml_link
wget -O credentials.json $creds_link
wget -O settings.yaml $setting_link

fileid="1cPf8wL_XeYnqwFw1GEz_lDaBjR5I55Ra"
filename="build.zip"
query=`curl -c ./cookie.txt -s -L "https://drive.google.com/uc?export=download&id=${fileid}" |\
     perl -nE'say/confirm=(\w+)/'`
curl -Lb ./cookie.txt "https://drive.google.com/uc?export=download&confirm=${query}&id=${fileid}" -o ${filename}


# Unzip the Unity build folder
unzip  -o build.zip 

# 4. Run the Unity executable and train the ML Agents.
env_name="./HeadlessBuilds/build.x86_64"
trainer_config_file="./trainer_config.yaml"
run_identifier="test_1"

# Assuming the unity build is an executable. If it isn't, adjust the command to execute it appropriately.
chmod +x $env_name 
chmod +x $trainer_config_file
chmod +x HeadlessBuilds

python -c "

from mlagents_envs.environment import UnityEnvironment
env = UnityEnvironment(file_name='$env_name', no_graphics=True)

env.reset()

behavior_name = list(env.behavior_specs)[0]
spec = env.behavior_specs[behavior_name]


max_steps = 1000000  # Adjust this value based on your desired max steps
current_steps = 0

while current_steps < max_steps:
    env.reset()
    decision_steps, terminal_steps = env.get_steps(behavior_name)
    tracked_agent = -1 # -1 indicates not yet tracking
    done = False # For the tracked_agent
    episode_rewards = 0 # For the tracked_agent
    while not done:
        # Track the first agent we see if not tracking
        if tracked_agent == -1 and len(decision_steps) >= 1:
            tracked_agent = decision_steps.agent_id[0]

        # Generate an action for all agents
        action = spec.action_spec.random_action(len(decision_steps))

        # Set the actions
        env.set_actions(behavior_name, action)

        # Move the simulation forward
        env.step()
        current_steps += 1

        # Get the new simulation results
        decision_steps, terminal_steps = env.get_steps(behavior_name)
        if tracked_agent in decision_steps:  # The agent requested a decision
            episode_rewards += decision_steps[tracked_agent].reward
        if tracked_agent in terminal_steps:  # The agent terminated its episode
            episode_rewards += terminal_steps[tracked_agent].reward
            done = True
    print('Total rewards until now: ' + str(episode_rewards))


env.close()
print('Closed environment')
"
# ls -la

# 5. Zip the results file and upload it to Google Drive.
results_folder="results"
zip_file="${results_folder}.zip"
zip -r $zip_file $results_folder


python -c "
from pydrive.auth import GoogleAuth
from pydrive.drive import GoogleDrive

# Authentication
gauth = GoogleAuth()
# gauth.LoadCredentialsFile('credentials.json')
gauth.CommandLineAuth()

if gauth.credentials is None:
    print('Missing credentials.json')
    exit(1)

drive = GoogleDrive(gauth)

# Upload the zipped results file
file_drive = drive.CreateFile({'title': '$zip_file'})
file_drive.SetContentFile('$zip_file')
file_drive.Upload()
# print('Uploaded ' + zip_file + ' to Google Drive')
"
