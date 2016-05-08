# mya16
Custom cpu architecture with compiler. <br>
(circuit files are located in **`cpu/`** directory)

####How to compile assembly:
You have to use the `mp16.exe` compiler, if you want to see the sources, take a look inside of the **`sources/`** directory. If you have your sources spread to multiple files, target the root one.
```
mp16.exe path/to/my/root_file.mya16
```
Compiler will create a `.bin16` file from all source files.
<br>

####How to execute binary files:
You have following options <br>
**A) Run Logisim simulation** <br>
 1. You need to have [Logisim][1] installed
 2. You have to download the [cpu schematic][2] and open it up in Logisim
 3. You have to load the `.bin16` file into the ram. (R-click -> Load contents... -> select the file)
 4. Then you just start the clock (Ctrl-K) and press the button labeled **I/O**

**B) Run the emulator** <br>
If you are too lazy to run logisim every time you alter the sources, you can use the `mp16_interpreter.exe`. It executes the `.bin16` files the same way as the cpu simulation in Logisim would.
```
mp16_interpreter.exe path/to/my/compiled_file.bin16
```

[1]: http://www.cburch.com/logisim/
[2]: https://github.com/Muph0/mya16/blob/master/cpu/CPU-16IR.circ

#blue-os
Basic operating system written in Assembly dialect for my cpu architecture <br>
*(those .mya16 files are the sources, os.mya16 is the main one)*
