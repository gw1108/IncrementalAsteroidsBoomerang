# IncrementalAsteroidsBoomerang
A copy of Astro Prospector, but with a boomerang projectile. For researching how to build an entire game completely with Claude code. Then we use Bezi to refine the game if needed so we can submit to the game jam.

## Setup
Because Unity uses Unix LF line endings for serializing prefabs and meta assets we just leave them alone. Add the following line to %UserProfile%\.gitconfig.
[core]
    autocrlf = false

Using Unity 6000.3.8f1 Personal. https://unity3d.com/get-unity/download

Optionally select the following:
* Android Build Support (and OpenJDK and Android SDK & NDK Tools)
* iOS Build Support (if you are developing on a Mac)
* Mac Build Support (if you are developing on a Mac)
* Linux Build Support (IL2CPP)
* Windows Build Support (IL2CPP)
* Documentation
* Web Build Support

Set your File->Build Profiles to Web.

### To Build WebGL
Uncomment:
```
config.autoSyncPersistentDataPath = true;
```
Then zip up the build contents.

## Miro board
https://miro.com/app/board/ - None

## Trello board
https://trello.com/ - None

## Google GDD
https://docs.google.com/ - None

## Contact
georgetw1108@gmail.com - George Wang
