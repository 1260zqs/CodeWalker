import argparse
import datetime
import os
import subprocess


def get_gta5exe_date(path: str):
    try:
        created_ts = os.path.getctime(path)
        created_dt = datetime.datetime.fromtimestamp(created_ts)
        return created_dt
    except Exception as e:
        return None

# Interactive sorting of packages
def interactive_sort(files):
    while True:
        print("="*100)
        print("The following packages were found:")
        for idx, f in enumerate(files):
            print(f"  [{idx}] {f}")
        print("\nYou can reorder the install order and skip packages.")
        print("Enter a comma-separated list of numbers in the desired order (e.g. 2,0,1). Omitted numbers will be skipped.")
        print("Each number can appear at most once. Or just press Enter to keep the current order.")
        order = input("New order: ").strip()
        if not order:
            return list(range(len(files))), files
        try:
            indices = []
            for part in order.split(","):
                part = part.strip()
                if not part:
                    continue
                if "-" in part:
                    start, end = part.split("-", 1)
                    start = int(start.strip())
                    end = int(end.strip())
                    if start > end:
                        raise ValueError("Range start greater than end")
                    indices.extend(list(range(start, end+1)))
                else:
                    indices.append(int(part))
            if any(i < 0 or i >= len(files) for i in indices):
                print("Invalid input: One or more numbers are out of range. Please try again.\n")
                continue
            if len(set(indices)) != len(indices):
                print(f"Invalid input: Duplicate numbers detected in {indices}. Please try again.\n")
                continue
            return indices, [files[i] for i in indices]
        except Exception as e:
            print(f"Invalid input: {e}. Please enter only numbers or ranges separated by commas. Try again.\n")


# fmt: off
parser = argparse.ArgumentParser(description="Run CodeWalker.OIVInstaller.exe on a .oiv file.")
parser.add_argument("--game", type=str, required=True, help="Path to GTA V game folder (containing GTA5.exe)")
parser.add_argument( "--installer", type=str, required=True, help="Path to CodeWalker.OIVInstaller.exe")
parser.add_argument("--package", type=str, required=True, help="Path to .oiv/.zip file or folder containing .oiv/.zip files")
args = parser.parse_args()
# fmt: on
path_installer = args.installer
path_package = args.package
path_game = args.game

ALLOWED_PACKAGE_EXTENSIONS = [".oiv", ".zip"]

# Assert GTA5.exe exists in the game folder if provided
assert os.path.isfile(os.path.join(path_game, "GTA5.exe")), (
    "GTA5.exe not found in the specified game folder."
)
# Further path validations
assert path_installer.endswith("CodeWalker.OIVInstaller.exe"), (
    "Installer path must point to CodeWalker.OIVInstaller.exe."
)
assert (
    os.path.isdir(path_package)
    or any(path_package.lower().endswith(ext) for ext in ALLOWED_PACKAGE_EXTENSIONS)
), "Package path must be a .oiv/.zip file or a folder containing .oiv/.zip files."

# fmt: off
print()
print("="*100)
print(" OIV Package Installer ")
print()
print(f"Game Folder: {path_game} (Created: {get_gta5exe_date(os.path.join(path_game, 'GTA5.exe'))})")
print(f"Installer: {path_installer}")
print(f"Package(s): {path_package}")
print()
# fmt: on


# Determine if path_package is a folder or file
oiv_files = []
if os.path.isdir(path_package):
    packages = [
        os.path.join(path_package, f)
        for f in os.listdir(path_package)
        if any(f.lower().endswith(ext) for ext in ALLOWED_PACKAGE_EXTENSIONS)
    ]
    if not packages:
        print("No .oiv or .zip files found in the folder.")
        exit(1)
else:
    packages = [path_package]


indices, packages = interactive_sort(packages)


# Show final order and confirm again
print("="*100)
print("\nFinal install order:")
for idx, package in zip(indices, packages):
    print(f"  [{idx}] {package}")
print()
confirm = input("Proceed with installation in this order? (y/N): ").strip().lower()
if confirm != "y":
    print("Aborted by user.")
    exit(0)

for package in packages:
    print(f"Installing: {package}")
    cmd = [path_installer, "--install", package, "--game", path_game]
    result = subprocess.run(cmd)
    print()
    if result.returncode == 0:
        print(" Installation completed successfully!")
    else:
        print(f" Installation failed with error code: {result.returncode}")
        
        choice = input("Continue with next package? (y/N): ").strip().lower()
        if choice != "y":
            print("Aborted by user due to error.")
            exit(1)
