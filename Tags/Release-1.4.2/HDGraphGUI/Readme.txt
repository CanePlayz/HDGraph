HDGraph (http://www.hdgraph.com)
=========================================

[French version at the bottom !]
[Version Fran�aise plus bas !]


Minimum requirements
======================================================

- Windows XP or Windows Vista or Windows 7 or Windows 8 or Any system with Mono (Linux, Mac OS, etc.)
- For Windows XP, the Microsoft .NET Framework 2.0 (HDGraph Setup wil propose you to download it, if it's not installed).
- Recommanded (but not required) : in order to use the evolved draw engine, you must have the version 3.5 SP1 of the .NET Framework.

	
Install on WINDOWS :
======================================================

- Make sure your computer satisfy the minimum requirements.
- Start the installation file (setup file) and follow the instructions.
OR
- Unzip the Zip file and start HDGraph.exe


Install on Linux, Mac OS : 
======================================================

- You must install Mono if it's not installed already. (Version 2.6.1 or higher is recommended. HDGraph has NOT been tested with an older version of Mono). Download Mono at http://mono-project.com .
- Unzip the HDGraph Zip file somewhere on your hard disk.
- Open a shell and execute, in the HDGraph folder, the following command (without quotes) :   "mono HDGraph.exe"
	

Uninstall on WINDOWS :
======================================================

First of all : 
- If you previously activated the HDGraph "windows explorer integration", it is better to deactivate it first (open HDGraph and go to the menu item "Tools > Explorer Integration >  Remove me from the explorer context menu").
- Make sure you closed all opened HDGraph windows.

Next :
- if you used the Zip package to install HDGraph (for example the "HDGraph_light_x.y.z.zip" file), you just have to delete the HDGraph folder.
- if you used the MSI file (for example "SetupHDGraph_English_x.y.z.msi"), HDGraph is in the uninstall software list of the control panel and you can uninstall it from here : 
	1/ Open "Add/Remove programs" from the Configuration Panel, 
	2/ select HDGraph in the list and clic "Remove"


Known bugs and limitations:
======================================================

- HDGraph is unable to scan files and directories which full path is greater thant 255 characters.
- Using HDGraph on Mono is experimental :
-    Some feature are not available
-    There are many visual bug yet
-    The evolved draw engine (using WPF) is not and will never be available (because WPF is not implemented in Mono). Only the basic draw engine is available.

Contacts:
======================================================

Last informations about HDGraph and its contact informations can be found at :
http://www.hdgraph.com

	
	
	


	

/*****************************************************
*
*				VERSION FRANCAISE
*
******************************************************/


	
Configuration requise
======================================================

- Windows XP ou Windows Vista ou Windows 7 ou Windows 8 ou tout syst�me compatible avec Mono (Linux, Mac OS, etc.)
- Pour Windows XP, le .NET Framework 2.0 (Le Setup HDGraph vous proposera de le t�l�charger automatiquement si vous ne l'avez pas).
- Recommand� (mais facultatif) : pour utilis� le dernier moteur de dessin WPF, vous devez avoir la version 3.5 SP1 du framework .NET.


Installation sous WINDOWS :
======================================================

- Assurez vous que votre ordinateur poss�de au moins la configuration requise pour installer le logiciel.
- Lancez le fichier d'installation et suivez les instructions indiqu�es � l'�cran.
OU
- D�compressez le fichier ZIP dans un dossier puis lancez le fichier HDGraph.exe.

Installation sous Linux, Mac OS :
======================================================

- Vous devez installer Mono s'il n'est pas d�j� install�. La version 2.6.1 ou sup�rieure est recommand�e. HDGraph n'a PAS �t� test� avec une version ant�rieur de Mono. Mono est disponible sur http://mono-project.com .
- D�compresser le fichier Zip de HDGraph sur le disque dur.
- Ouvrir un shell, se positionner dans le r�pertoire o� a �t� extrait le Zip HDGraph, et ex�cuter la commande suivante (sans les guillemets) :   "mono HDGraph.exe"


D�sinstallation sous WINDOWS:
======================================================

Avant toute chose :
- Fermez toutes les fen�tres HDGraph ouvertes.
- Si vous avez dans le pass� activ� l'int�gration d'HDGraph � l'explorateur, il est pr�f�rable de d�sactiver cette option avant de d�sinstaller.(ouvrir HDGraph and go to the menu item "Tools > Explorer Integration >  Remove me from the explorer context menu").

Ensuite :
- si vous avez install� HDGraph avec un fichier Zip (par exemple "HDGraph_light_x.y.z.zip"), il vous suffit de supprimer le dossier dans lequel vous avez d�compress� le fichier.
- si vous avez install� HDGraph avec un installeur MSI (par exemple "SetupHDGraph_Francais_x.y.z.msi"), vous pouvez d�sinstaller HDGraph via le panneau de configuration : 
	1/ Ouvrir le Panneau de configuration > Programmes > Programmes et fonctionnalit�s
	2/ selectionner HDGraph dans la liste et cliquer sur "D�sinstaller"



Bug connus et limitations :
======================================================

- HDGraph est incapable de scaner les fichiers et r�petoires dont le chemin exc�de 255 caract�res.
- Sous Mono : version EXPERIMENTALE :
-	Le nombre de fonctionnalit�s est restreinte
-   Il y a encore de nombreux bugs visuels
-   Le moteur de dessin �volu� (en WPF) n'est pas et sera jamais disponible (WPF n'est pas impl�ment� dans Mono). Seul le moteur de dessin basique fonctionne.


Contacts:
======================================================

Les derni�res informations � propos de HDGraph sont disponibles sur le site internet: 
http://www.hdgraph.com
