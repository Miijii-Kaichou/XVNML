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

## How to Get Started
You can get started with using XVNML in a variety of ways. Since XVNML was made using C#, you can apply it to any .NET Application you want, whether that'd be on a C# Console, WPF, or ASP.NET application.
You can even integrated into Unity, and make it part of your workflow. All it takes is adding the XVNML NuGet package to your project, and you're ready to go! For starters, you can reference off a C# Console Application sample [here](https://github.com/Miijii-Kaichou/XVNML/tree/main/XVNMLTest)! You may also want to check out an article I wrote on Medium that walks you through how to install XVNML and integrated in both a C# Console Application and a Unity project [here](https://medium.com/@miijii/get-started-using-xvnml-cc6dc478fc8e)!

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
A Proxy file is always your main file, and is always read first. However, a Source is a file that can be used to introduce modularity into your projects. For example, the screenshot below shows that we’re creating a Cast element called Hana, and the source of that data is in “Hana\Hana.cast.xvnml”

![image](https://miro.medium.com/v2/resize:fit:720/format:webp/1*i58PbZYcGJZ62Nph5_JBkg.png)

![image](https://miro.medium.com/v2/resize:fit:720/format:webp/1*4iQRmnI1JJt4OffM6IGrsg.png)

By utilizing Source files, you can reduce the number of lines in your proxy file. It’s one of the most important best practices of using XVNML in your development process.

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

Skriptr is the syntaxical language used in tandem with XVNMl to output text data. Each Skriptr line must first be given a role. It can either take in a Declaratice Role (denoted by the "@" symbol), oran Interrogative Role (denoted by the "?" symbol).
When given a Declarative Role, it'll allow the user to, for example, output text to the screen. However, if given an Interrogative Role, the dialgoue comes a Prompt that requires user input.

You are also able to run macros (commands that performs a single action). Information of the command used, as well as the arguments passed will be stored inside your XVNMLObj instance. A macro is denoted by surrounding curly braces. The macro name is first defined, and then the argument(s) being passed for it (if the macro requires them).
 
Another feature with Skriptr is the ability to name lines using square brackets. Combining macros such as "jump_to" and "lead_to", you're able to use named lines to control the flow of dialogue.

```xvnml
$> XVNML Example
["P1-Skriptr-Incorrect"]
@ Unfortunately, that's not the correct answer...{jump_to::"P1-Skriptr-CorrectAnswer"}<

["P1-Skriptr"]
@ Skriptr is the syntaxical language used in tandem with XVNML to...<
@ type out dialogue.<
@ Each Skriptr line must first be given a role to play.<
@ It can either take in a Declarative Role {paren}denoted by the {quot|at|quot} symbol{paren_end}...<
@ Or a Interrogative Role {paren}denoted by the {quot|qm|quot} symbol {paren_end}...<
@ When given a Declarative Role, it'll type out dialogue to the screen as normal...{delay::500|clr}
Just like what you are seeing now.<
@ However, if given a Interrogative Role...<
@ The dialogue acts as a Prompt in which the user must answer to.<
? For example, how many states does the United States have?>>
(
     ("50")>
          @ That is correct!<<
     ("13")>
          @ {jump_to::"P1-Skriptr-Incorrect"|pass}<<
     ("3")>
          @ {jump_to::"P1-Skriptr-Incorrect"|pass}<<
     ("I honestly don't know...")>
          @ That's okay if you don't know.<<
)

["P1-Skriptr-CorrectAnswer"]
@ There are 50 states that makes up the United States of America.<
```
)
