# LWMS
LWMS is shot for **L**ight**W**eight **M**anaged **S**erver

## How lightweight LWMS is?
Including `conhost.exe`, it occupies only 12 MB memory on Windows 10 when there's no burden.

## Difference Between LWMS and LWSwnS
LWMS uses pipeline by design while LWSwnS uses event model only. 
In the meanwhile, LWSwnS only support static text file transmission by default if not installing BFT module while LWMS support any file transmission.
Also, LWMS uses HttpListener as its back-end a higher-level class while LWSwmS uses TcpListner which is complex to use.