# XVNML

XVNML (formally known as X-tensible Visual Novel Markup Language), is an open-source mark-up language that can be easily paired with any game engine that requires a nice and structure way of storing Stored-Based information and dialogue. It puts together the normal mark up languages you may be used to, but adds a couple extra features (like Skriptr) to make it versitale in you game development work flow.

![Screenshot_20221205_234724](https://user-images.githubusercontent.com/46007223/217719313-597ad1ae-2ee0-4348-abe5-19ba43b8445f.png)

![Screenshot_20221030_161330](https://user-images.githubusercontent.com/46007223/217717794-20048095-750e-4709-a918-aa0b0897281b.png)

## Roadmap

At it's current stage, these are the following states that this project is in:

  * You can easily import the .dll of XVNML, and add it into any .NET-based game engine or environment (such as Unity).
  * For Unity in particular, there will be a free package that demonstrates and utilizes XVNML called XVNML2U.
  * There still needs to be helpers to make integrating to game engines a lot easier.
  * There will be a Native Visual Novel Game Engine that'll be fully dedicated to XVNML and Skriptr called HanaC.
  * The XVNMLExt for VSCode is yet to be polished as of the time of writting this.

## How to Use XVNML

All of your XVNML content are represented by an object called XVNMLObj. You instaniate this class, and pass it in a directory to a file you want it to parse. It'll then go through and parse the information for you, as well as extra any Skriptr used for dialogue. After the parsing, you can grab an element by using the GetElement method on the root tag of the XVNML file.

![image](https://user-images.githubusercontent.com/46007223/217720058-c378e51b-200a-41f9-a773-3f6c04e882bd.png)

### Pre-Built Tags
By default, you have a total of 28 pre-built tags that can be parsed by the XVNML utility.
  * Audio
  * AudioDefinition
  * Author
  * Cast
  * CastDefinition
  * Copyright
  * Date
  * Dependency
  * DependencyGroup
  * Description
  * Dialogue
  * DialogueGroup
  * Image
  * ImageDefinitions
  * Keycode
  * KeycodeDefinitions
  * Metadata
  * Portrait
  * PortraitDefinitions
  * Proxy
  * Scene
  * SceneDefinition
  * Source
  * Tags
  * Title
  * Url
  * Voice
  * VoiceDefinition
  
Out of all of these valid tags, Proxy and Source are Root Tags. This means whenever you type your XVNMLObj instance and access the root, it'll either be Proxy or Source, depending on how the file was structured.

### User Defined Tags

You also have the ability to create custom tags (hence the X in XVNML). Creating a custom class is as easy as associating your C# class with a particular tag.

![image](https://user-images.githubusercontent.com/46007223/217721390-fcd2030d-054d-44e9-90fb-7da8330c5553.png)

Once you've created a class that derives from the UserDefined class, you can associate that class with a tag inside your .XVNML file. You can even specifiy its scoop:
  * PragmaOnce => Should only be mentioned 1 time in a file.
  * PragmaLocalOnce => Should only be mentioned 1 time inside a scope of another tag.
  * Multiple => Can be used anywhere, and can be use multiple times thoughout the whole .XVNML document

### XVNML file as Sources and Resource Referencing
XVNML files can also be used as sources for other tags. Tag the list of cast members in the CastDefinition tag:
![image](https://user-images.githubusercontent.com/46007223/217722427-979a6dd9-6e9b-4a67-aab7-1e4565c56f04.png)
Using a configuration file to find out the relative path of your project, you can take the information from one XVNML and embed it into a tag. For example, this is what the information looks like inside "Raven.cast.xvnml":

![image](https://user-images.githubusercontent.com/46007223/217722755-c759a5d7-cf69-4df4-a4c7-38f3cffeee3b.png)

It'll take the information from the "cast" tag inside the Raven.cast.xvnml file, and then embed it into the cast tag from the requesting file. It's a very awesome feature exclusive to XVNML. However, the only downside to this is that the "name" parameter for the "cast" tags in both files must match. They will fail to match otherwise. 

### Skriptr

Skriptr is a scripting language entirely unique to XVNML. In order to utlize this handy feature, all of it's contents must be inside a "dialogue" tag. Again, you can have another XVNML file as a source and have the dialogue embed to the dialogue asking for it that way (again, the names of those tags must both match).

![image](https://user-images.githubusercontent.com/46007223/217723301-61a37ee5-6957-4573-91f2-60b4216c9dd9.png)

You are also able to run macros (commands that performs a single action). Information of the command used, as well as the arguments passed will be stored inside your XVNMLObj instance. A macro is denoted by surrounding curly braces. The macro name is first defined, and then the argument(s) being passed for it (if the macro requires them).

![image](https://user-images.githubusercontent.com/46007223/217723765-c4ed8f8f-4251-4cdf-b2db-5bc341ddbe3c.png)

## Documentation/Learning Material

To learn more about XVNML, and how you can integrate it inside a game engine (or your own game engine), be sure to check out the wiki. There will be extensive detail as to how everything functions, and the approach to take when implementing it into your project.
Again, there will be different subprojects that takes XVNML and addes support to already existing game engines; the first one being Unity with XVNML2U. Be sure to keep an eye out for updates.

There will also be a VSCode extension of XVNML coming soon (which is simply just the syntax highlighting). I plan on creating a website dedicated to learning XVNML, Skriptr, adding macros, setting the proxy (target) and mroe. But in the meantime, be sure to support the development of this mark-up language.

