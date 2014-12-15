Readme
------

 Enviroment  
 OS: Windows 8.1 Pro  
 Moder UI apps:  
 http://apps.microsoft.com/windows/en-us/app/8289549f-9bae-4d44-9a5c-63d9c3a79f35  
 http://apps.microsoft.com/windows/en-us/app/add3d66a-358d-4fe2-be68-8a3f934e9ea1  
 http://apps.microsoft.com/windows/en-us/app/97a2179c-38be-45a3-933e-0d2dbf14a142


Steps to reproduce the memory leak (**TestUIA_MemoryLeak**)  
1. Run TestUIA_MemoryLeak app  
2. Run Twitter app, move mouse inside the app window, scroll it, click on it  
3. Close twitter app with cross I the right-up corner  
4. Run Facebook(fb) app, move mouse under fb app window, scroll content, click on posts  
5. Close fb app with cross I the right-up corner  
6. Run maps app, move mouse inside the app window, scroll around, and drag the content  
7. Close maps app with cross I the right-up corner  
8. Repeat last 6 steps in random order and after a while you will see that memory usage start growing  
 
If we add dispose for UI elements and call Marshal.FinalReleaseComObject in it after a while AutomationElement.FromPoint returns only root window ( this is shown in **TestUIA_StopAnswer** app)

