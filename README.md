# DLX-public-health-inspection
CVRI restaurant inspection simulation

# Deployment
See our Restaurant Inspection course at [Contact North](https://xrprojects.contactnorth.ca)

# Documentation
Developer page in [DLX-confluence](https://github.com/CC-Open-Learning/DLX-confluence/blob/main/LSM9.01---Public-Health-Inspection_805306369.html)

# Developer docs
Setting up (UPM Packages)[https://github.com/CC-Open-Learning/VARLab-confluence/blob/main/Accessing-the-Package-Registry-for-Developers_783614144.html] for Unity development if needed

Creating a (WebGL SCORM)[https://github.com/CC-Open-Learning/CORE-confluence/blob/main/CV2/521076742.html] build.

* This project overrides Library/PackageCache/com.varlab.cloudsave@1.0.0/Runtime/Scripts/AzureSaveSystem.cs, so that file specifically needs to be copied from the repository and pasted over the same in the local clone after Unity downloads UPM packages when setting up the development environment.
* Additionally, SaveObject properties tend to reset in the development environment so make sure it has AzureSaveSystem and LocalSaveSystem scripts attached as components and those components dragged to their proper spots in SaveObject.

# Contributors
Salman Nouman Abulqasim,
Ryan Samii,
Carolina Naoum Junqueira,
Juhwan Seo,
Stacey Dineen,
Topher Rouleau,
Sana Javeed,
Aidan Cheesmond,
Jonathan Bezeau,
Islam Ahmed,
Netra Hindocha,
Omar Nunez Siri,
Talon Ernst,
Hamna Ashraf,
Jindo Kim,
Benjamin Smith,
Nathan Joannette,
Emanuel Juracic,
Karandeep Sandhu,
Blake Hadaway,
Justin Schulz,
Julian Cumming,
Ali Kaya,
Allison Bielaski
