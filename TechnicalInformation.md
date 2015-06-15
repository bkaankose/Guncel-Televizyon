# Technical Information

* This solution has 2 projects , Windows 8.1 and Windows Phone 8.
* Both projects use PhoneSM (Windows Phone Streaming Media - https://phonesm.codeplex.com/) and you need to build this project and add missing references to needed project.
* Windows Phone app uses some Telerik controls , you might need those references as well.
* Both projects use SQLite for storing & managing local data.Install Windows Runtime Extension for SQLite before you build it.You can access local db file (gtv.db) under Database folder in both projects.
* Both projects have some removed & forgotten features , like Push Notification support.I'm not motivated to remove all of them ...

I am very aware of project architecture is a bit patchy , but still one of the best in the Windows Store.Actually it's the only app that is still working and actively being developed.

The reason behind this patchiness is I didn't spend much time on architecture and mostly focues on coding part.I'm thinking to re-code whole project in MVVM Light , if I'm lucky enough to get some freetime out of life !

Enjoy :)