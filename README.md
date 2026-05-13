
# KeyOverlay
Awesome edit that makes it an actual overlay rather than a bordered window

(need to change config to move it sorry)
 
![]()![2024-10-25_19-35-57-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/b7c53d8b-0a1a-471a-90b2-f6c4628fba5a)

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



