# Archive Viewer

Archive Viewer is a program to view and edit .archive files, which Unreal Engine 4 uses to save its' localization data.


![archive viewer interface][archive_viewer_1]

[archive_viewer_1]: https://github.com/Unavi/ArchiveViewer/blob/master/Images/ArchiveViewer_1.png

Select the Game.manifest Unreal generates for you and bookmark it for later under a name of your choosing.

Select the language you want to translate and enter the translated text into the Translated column. After you are done press the Save Changes back to archive button.

Aside from editing the translations in the Archive Viewer you can export and import them with the two lower left buttons.

After you saved the changes press __Compile Text__ from your Localization Dashboard and the updated text should appear when you change the language and start the game in standalone mode.

#### Automatic translation
If you add a file named GoogleKey.json in the folder of the ArchivViewer.exe with a [Google Translation API](https://cloud.google.com/translate/docs/) Key inside, you can translate text automatically into the disired language with the buttons __Translate Selected__ and __Reverse translate selected__ to check if everything is properly translated in languages where you might otherwise not be able to read the letters.

You still have to hire people to properly translate your game but it is good enough for testing.