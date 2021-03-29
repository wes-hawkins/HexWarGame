using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadManager : MonoBehaviour {

    public static SaveLoadManager Inst = null;

	private string defaultDirectory { get { return (Application.persistentDataPath + "/SaveData").Replace("/", "\\"); } }
	private string currentDirectory;

	[SerializeField] private Transform filesContainer = null;
	[SerializeField] private TextMeshProUGUI directoryText = null;
	[SerializeField] private Button gameDirectoryButton = null;

	[Space]
	[SerializeField] private FileBrowserEntry upDirectoryEntry = null;
	[SerializeField] private GameObject directoryEntrySource = null;
	[SerializeField] private GameObject fileEntrySource = null;

	private List<FileBrowserEntry> dirEntries = new List<FileBrowserEntry>();
	private List<FileBrowserEntry> fileEntries = new List<FileBrowserEntry>();


	private void Awake() {
		Inst = this;
	} // End of Awake().


	public void ManualStart() {
		directoryEntrySource.SetActive(false);
		fileEntrySource.SetActive(false);

		currentDirectory = defaultDirectory;
		Populate(defaultDirectory);
	} // End of ManualStart() method.


	// Refreshes the file list to a given directory.
	private void Populate(string directory){
		if(!Directory.Exists(defaultDirectory))
			Directory.CreateDirectory(defaultDirectory);

		DirectoryInfo directoryInfo = new DirectoryInfo(directory);
		directoryText.SetText(directoryInfo.FullName);

		gameDirectoryButton.interactable = directory != defaultDirectory;
		Debug.Log("directory1: " + directory);
		Debug.Log("directory2: " + defaultDirectory);

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
			FileBrowserEntry newFileEntry = Instantiate(fileEntrySource, filesContainer).GetComponent<FileBrowserEntry>();
			newFileEntry.SetLabel(file.Name);
			fileEntries.Add(newFileEntry);
			newFileEntry.gameObject.SetActive(true);
		}
	} // End of Populate() method.


	public void Button_GameDirectory(){
		Populate(defaultDirectory);
	} // End of Button_GameDirectory() method.

} // End of SaveLoadManager().
