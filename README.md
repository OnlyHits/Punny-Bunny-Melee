# GMTK-2025

### Open the project

The project is using `Unity 6000.0.51f1` the **LTS** version.<br/>
Make sure to install this version from [Unity hub](https://docs.unity3d.com/hub/manual/InstallHub.html).<br/>
The open the project you've just clonned with : 

```
git clone <github-url>
```

### Basic Setup

The project is using an external architecture, we've called the [CustomArchitecture](https://github.com/OnlyHits/CustomArchitecture) that you can see over here if you have the access rights.<br/>
The project is using it as a [git submodule](https://git-scm.com/docs/git-submodule). A [Makefile](https://www.gnu.org/software/make/manual/make.html) has been done to setup the architecture in the project.<br/>
Here are the usefull commands you can execute at the project's root :

```bash
make init     # First-time setup (the command you're looking for)
```

```bash
make update   # Pull latest submodule changes
```

```bash
make clean    # If you all messed up, it removes the submodule folder only
```

```bash
make help    # If my explanations are not enought
```

***Note:*** If you're using Windows and powershell does not recognize `make` or `git` commands, see the trouble [shooting section](#trouble-shooting) in the document.


### Trouble Shooting


Install `make` (for Windows users) :<br/>
1. Open powershell as administrator<br/>
2. Run this command : 
```bash
winget install ezwinports.make
```
3. Close your terminal & re open it, then you can run the commands

Install `git` (for Windows users) :<br/>
1. Open powershell as administrator<br/>
2. Run this command : 
```bash
winget install --id Git.Git -e
```
3. Close your terminal & re open it, then you can run the commands
