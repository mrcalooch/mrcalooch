   M S B A M L   `   `   `  ����  Nanopath     Nanopath.View.SplashScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      c _System.Windows.Interactivity, Version=4.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35Bi:http://schemas.microsoft.com/expression/2010/interactivity  5      
�������  � DataContext  5      �� 5   
      Locator �� Source# [  6    �� Path$ SplashViewModel�64    � 	Resources 5       �� Source�� 5   
   $ StyleResources.xaml?�6   %    .  (System.Windows.Interactivity.Interaction   Triggers 5      /  )System.Windows.Interactivity.EventTrigger  5   
      	EventName$ Loaded��6   .  (System.Windows.Interactivity.TriggerBase   Actions. 5      6  0System.Windows.Interactivity.InvokeCommandAction     Command 6$   �� *UserControlLoadedCommand+5        5   
   $ Unloaded��6   . 5         6$   �� *UserControlUnloadedCommand+5      .��5      � $��60,10,60,10q�6   	 � RowDefinitions	 5   
   �� 5      $��8* �6   �� 5      $��1* �6   .+�5!   
   �� $��0��6   $��Center=�6   $��Center=�6:   ��5"      �� 5#      
 �� 	UriSource$#
 Images/Nanopath Full Logo.png?�6   6J   5%      �� 5&   
   $��1��6   $��Center=�6!   $��Bottom=�6>   $��28(�5'      
���   ���5(      �� *SoftwareVersion+ �� Mode$ OneWay=� . *VersionStringToPresentationStringConverter �� 	Converter# [ 5)      5*      