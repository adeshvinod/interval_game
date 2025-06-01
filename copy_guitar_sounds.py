import os
import shutil
from pathlib import Path

# Source directory containing guitar dataset folders
source_dir = r"C:\Users\adesh\Downloads\archive\Guitar Dataset"

# Destination directory in Unity project
dest_dir = r"C:\Users\adesh\Documents\GitHub\interval_game\Assets\WordQuiz\Sound\guitar sounds"

# Create destination directory if it doesn't exist
os.makedirs(dest_dir, exist_ok=True)

# Counter for copied files
copied_files = 0

# Iterate through each folder in the source directory
for folder_name in os.listdir(source_dir):
    folder_path = os.path.join(source_dir, folder_name)
    
    # Check if it's a directory
    if os.path.isdir(folder_path):
        # Get all files in the folder
        files = os.listdir(folder_path)
        
        if files:  # If folder is not empty
            # Get the first file
            first_file = files[0]
            source_file = os.path.join(folder_path, first_file)
            
            # Create destination file path
            dest_file = os.path.join(dest_dir, first_file)
            
            try:
                # Copy the file
                shutil.copy2(source_file, dest_file)
                print(f"Copied: {first_file}")
                copied_files += 1
            except Exception as e:
                print(f"Error copying {first_file}: {str(e)}")

print(f"\nTotal files copied: {copied_files}") 