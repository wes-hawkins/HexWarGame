using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadManager : MonoBehaviour {

    public static SaveLoadManager Inst = null;

	private string defaultDirectory { get { return (Application.persistentDataPath + "/SaveData").Replace("/", "\\"); } }
    public string SaveFilePath(string saveName) { return PlayerPrefs.GetString(workingDirectoryPlayerPref, defaultDirectory) + "/" + saveName + scenarioFileExtension; }

	private string workingDirectoryPlayerPref = "workingDirectory";
	private string scenarioFileExtension = ".scenario";

	private string currentWorkingTitle = ""; // Name of the current scenario, for seeding save game text field.

	[SerializeField] private GameObject fileBrowserWindow = null;

	[SerializeField] private Transform filesContainer = null;
	[SerializeField] private TextMeshProUGUI directoryText = null;
	[SerializeField] private Button gameDirectoryButton = null;
	[SerializeField] private Button confirmButton = null;
	[SerializeField] private TextMeshProUGUI confirmButtonText = null;
	[SerializeField] private TMP_InputField filenameField = null;

	[Space]
	[SerializeField] private FileBrowserEntry upDirectoryEntry = null;
	[SerializeField] private GameObject directoryEntrySource = null;
	[SerializeField] private GameObject fileEntrySource = null;

	private List<FileBrowserEntry> dirEntries = new List<FileBrowserEntry>();
	private List<FileBrowserEntry> fileEntries = new List<FileBrowserEntry>();

	private enum FileBrowserMode { LOADING, SAVING }
	private FileBrowserMode mode;



	private void Awake() {
		Inst = this;
	} // End of Awake().


	public void ManualStart() {
		directoryEntrySource.SetActive(false);
		fileEntrySource.SetActive(false);

		gameObject.SetActive(false);
	} // End of ManualStart() method.


	// Refreshes the file list to a given directory.
	private void Populate(string directory){
		if(!Directory.Exists(defaultDirectory))
			Directory.CreateDirectory(defaultDirectory);
		PlayerPrefs.SetString(workingDirectoryPlayerPref, directory);
		filenameField.SetTextWithoutNotify("");

		DirectoryInfo directoryInfo = new DirectoryInfo(directory);
		directoryText.SetText(directoryInfo.FullName);

		gameDirectoryButton.interactable = directory != defaultDirectory;

		if(directoryInfo.Parent != null){
			upDirectoryEntry.SetLabel("..\\" + directoryInfo.Parent.Name);
			upDirectoryEntry.Button.onClick.RemoveAllListeners();
			upDirectoryEntry.Button.onClick.AddListener(delegate{ Populate(directoryInfo.Parent.FullName); });
		}

		foreach(FileBrowserEntry entry in dirEntries)
			Destroy(entry.gameObject);
		dirEntries.Clear();
		DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
		foreach(DirectoryInfo subDirectory in subDirectories){
			FileBrowserEntry newDirEntry = Instantiate(directoryEntrySource, filesContainer).GetComponent<FileBrowserEntry>();
			newDirEntry.SetLabel(subDirectory.Name);
			dirEntries.Add(newDirEntry);
			newDirEntry.gameObject.SetActive(true);
			newDirEntry.Button.onClick.AddListener(delegate{ Populate(subDirectory.FullName); });
		}

		foreach(FileBrowserEntry entry in fileEntries)
			Destroy(entry.gameObject);
		fileEntries.Clear();
		FileInfo[] files = directoryInfo.GetFiles();
		foreach(FileInfo file in files){
			if(file.Extension == ".scenario"){
				FileBrowserEntry newFileEntry = Instantiate(fileEntrySource, filesContainer).GetComponent<FileBrowserEntry>();
				newFileEntry.SetLabel(Path.GetFileNameWithoutExtension(file.Name));
				fileEntries.Add(newFileEntry);
				newFileEntry.gameObject.SetActive(true);
				newFileEntry.Button.onClick.AddListener(delegate{ FileSelected(Path.GetFileNameWithoutExtension(file.Name)); });
			}
		}
	} // End of Populate() method.


	private void FileSelected(string filename){
		filenameField.SetTextWithoutNotify(filename);
		UpdateInputFieldInteractibility();
	} // End of FileSelected().


	public void Button_GameDirectory(){
		Populate(defaultDirectory);
	} // End of Button_GameDirectory() method.


	public void Button_Confirm(){
		switch(mode){
			case FileBrowserMode.LOADING:
				LoadGame(filenameField.text);
				break;
			case FileBrowserMode.SAVING:
				SaveGame(filenameField.text);
				break;
		}
	} // End of Button_Confirm().


	public void SaveGameMenu(){
		UIOverlay.Inst.Show(true);
		fileBrowserWindow.SetActive(true);
		mode = FileBrowserMode.SAVING;
		confirmButtonText.SetText("Save");
		SeedDirectoryAndFilename();
	} // End of SaveGame().


	public void LoadGameMenu(){
		UIOverlay.Inst.Show(true);
		fileBrowserWindow.SetActive(true);
		mode = FileBrowserMode.LOADING;
		confirmButtonText.SetText("Load");
		SeedDirectoryAndFilename();
	} // End of LoadGame().




	private void SaveGame(string saveName){
		// Generate save data
		SaveGlob saveGlob = new SaveGlob();

		// Set save glob stuff here...
		// ...

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;
		if(File.Exists(SaveFilePath(saveName)))
			File.Delete(SaveFilePath(saveName));
		file = File.Create(SaveFilePath(saveName));
		bf.Serialize(file, saveGlob);
		file.Close();

		Debug.Log("Saved " + saveGlob.ScenarioName + " to \"" + SaveFilePath(saveName) + "\".");
		currentWorkingTitle = saveName;
		Close();

	} // End of SaveGame().



	private void LoadGame(string saveName){
		Debug.Log("Attempting to load " + SaveFilePath(saveName) + "...");
		if(File.Exists(SaveFilePath(saveName))){

			// TODO: Clear everything here...
			// ...

			// Load and populate saved data
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(SaveFilePath(saveName), FileMode.Open);
			SaveGlob saveGlob = (SaveGlob)bf.Deserialize(file);

			Debug.Log("Loaded " + saveGlob.ScenarioName + ".");
			currentWorkingTitle = saveName;

			Close();
		} else {
			Debug.LogWarning("Couldn't find file: " + SaveFilePath(saveName));
		}
	} // End of SaveGame().


	private void Close(){
		UIOverlay.Inst.Show(false);
		fileBrowserWindow.SetActive(false);
	} // End of Close().


	// Opens the user's previous directory, and seeds the current scenario filename, if applicable.
	private void SeedDirectoryAndFilename(){
		Populate(PlayerPrefs.GetString(workingDirectoryPlayerPref, defaultDirectory));
		filenameField.SetTextWithoutNotify(currentWorkingTitle);
		UpdateInputFieldInteractibility();
	} // End of SeedDirectoryAndFilename() method.


	public void UpdateInputFieldInteractibility(){
		switch(mode){
			case FileBrowserMode.SAVING:
				confirmButton.interactable = filenameField.text.Length > 0;
				break;
			case FileBrowserMode.LOADING:
				confirmButton.interactable = File.Exists(SaveFilePath(filenameField.text));
				break;
		}
	} // End of UpdateInputFieldInteractibility().


	public void Button_Close(){
		Close();
	} // End of Button_Close().


	[System.Serializable]
	public class SaveGlob {
		internal string scenarioName = "New Scenario"; public string ScenarioName { get { return scenarioName; } }
	} // End of SaveGlob.

} // End of SaveLoadManager().
