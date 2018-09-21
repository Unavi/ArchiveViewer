# Archive Viewer

Archive Viewer is a program to view and edit .archive files, which Unreal Engine 4 uses to save its' localization data.


![archive viewer interface][archive_viewer_1]

[archive_viewer_1]: https://github.com/Unavi/ArchiveViewer/blob/master/Images/ArchiveViewer_1.png

#### Steps to get started

* Select the Game.manifest Unreal generates for you and bookmark it for later under a name of your choosing.

* Select the language you want to translate and enter the translated text into the Translated column. After you are done press the Save Changes back to archive button.

* Aside from editing the translations in the Archive Viewer you can export and import them with the two lower left buttons. Translation data is exported in UTF-8.

* After you saved the changes press __Compile Text__ from your Localization Dashboard in UE4 and the updated text should appear when you change the language and start the game in standalone mode.

##### Automatic translation

If you add a file named GoogleKey.json in the folder of the ArchiveViewer.exe with a [Google Translation API](https://cloud.google.com/translate/docs/) Key inside, you can translate text automatically into the desired language with the buttons __Translate Selected__ and __Reverse translate selected__ to check if everything is properly translated in languages where you might otherwise not be able to read the letters.

You still have to find people to properly translate your game but it is good enough for testing.

#### Download

Archive Viewer 1.0
[Setup (Google Drive)](https://drive.google.com/open?id=10LkuDX48XDtfKPnD286QWKK7VzsAYj8r)
[Portable (Google Drive)](https://drive.google.com/open?id=1CCg-2jrJvho9M6GMX1NBjpnac_qerdRe)

\*You might need to install .NET Framework 4.6.1, if you don't have it or a later version already.