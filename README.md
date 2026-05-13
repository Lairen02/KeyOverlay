# KeyOverlay

> This project is modified based on [birbattac/KeyOverlay-birb-edit-](https://github.com/birbattac/KeyOverlay-birb-edit-).

Awesome edit that makes it an actual overlay rather than a bordered window

This custom overlay version changes the original behavior where the overlay position had to be edited through config. The overlay can now be dragged directly, and the window position can also be locked when you do not want it to move.

Right-click the application to open the quick settings menu:

- Lock position
- Hide overlay
- osu mode (A S)
- mania mode (D F J K)
- Close application

If you need to change the actual keys, edit `config.txt`.

<img width="620" height="344" alt="ManiaView" src="https://github.com/user-attachments/assets/029237ec-88df-4a79-969e-23b2bb8f23e3" />

(full desktop video no obs overlay required)

# config.txt properties
keyAmount - The amount of keys in the program (see the readme.txt for recommended widths for certain keyAmounts).

key1, key2 ... - Keys the program should use (UPPERCASE), for numbers and symbols [please refer to this table](https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Keyboard.php#acb4cacd7cc5802dec45724cf3314a142), for mouse buttons add m before the [mouse button options](https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Mouse.php#a4fb128be433f9aafe66bc0c605daaa90) ex. mLeft mRight. If you want more keys just add more fields.

displayKey1, displayKey2 - If the name of the key you are using is too large, or you would like to use a different symbol, you can use this property to override the original key name that is going to be displayed.

keyCounter - yes/no - Adds a keycounter beneath each key that counts total clicks in a session.

windowHeight, windowWidth - Used to change the resolution of the program.

windowPosX, windowPosY - Use to change the position of the overlay

keySize - Changes the size of the key (excluding border).

keyPressShrinkMult - Changes how much smaller the keys get on press

sizeFrames - Amount of frames it takes for the size to fully change

barSpeed - Adjusts the speed at which the bars are travelling upwards.

barOffsetY - Offsets where the bar spawns above the key

margin - Adjusts the margin of the keys from the sides.

outlineThickness - Changes the thickness of a square border

fading - yes/no - Adds/removes the fading effect on top 

backgroundColor, keyColor, PressFontColor, borderColor, barColor, fontColor - Changes the color of background (might be tricky, but possible to chroma key out in obs), key when not pressed, key when pressed, key border, bars and clicked key color, the font color using RGBA values.

backgroundImage - Lets you set a background. Put the image into Resources directory and then put the filename into this property ex. "bg.png" (without the quote symbols). Makes sure the background is the same resolution as your window and if you want transparency on your background you have to put the transparency on the image itself.

maxFPS - Sets the target FPS for the program to run
