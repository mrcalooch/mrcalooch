   M S B A M L   `   `   `  ����C  ?Nanopath, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null-clr-namespace:Nanopath.ViewNanopath.View  5      )    #Nanopath.View.CurrentSettingsScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:Nanopath.View     5      
�������$��28(�5	        � DataContext  5      �� 5   
      Locator �� Source# [  6    �� Path$ CurrentSettingsViewModel�64    � 	Resources 5       �� Source�� 5   
   $ ..\StyleResources.xaml?�6   %    .��5      � $��20q�6    � RowDefinitions 5   
   �� 5      $��64 �6   �� 5      $��* �6   .+�5   
      Nanopath.View.Header  $��0��6     5      �� 5      # [  6   $ CurrentSettingsViewModel�6<   5       � 5#   
   $��1��6   $��Center=�6   $��Center=�67   $��20q�6T    � ColumnDefinitions 5$      �� 5%      $��1* �6#   �� 5&      $��1* �6#    5)      �� 5*      $��1* �6    .+�5-      �� $��0��6   $��0��6"   
�������$��2q�6E   $��	0,20,20,0q�6Y   ._�5.      �� $��8q�6   $��24(�6'   $	��400��65    �� TextAlignment$
 Left=�6D   $
��Left=�5/      $	��Top=�67   $
��Wrap=�6O   ��50      �� *CurrentSettings+51      �� 52      $��0��6   $��1��6&    �� Orientation$ Vertical=�66   $��20q�6M   $��Center=�6Y   .+�53      �� $
��Edit��6   $��32(�6(   $��80��67   $	��300��6C   $��0, 50, 0, 25q�6O   ��54      �� *GoToSetupCommand+�� 55      $
��Next��6   $��32(�6(   $��80��66   $	��300��6B   $��0, 25, 0, 25q�6N   ��56      �� *GoToLoadSampleCommand+�� 57      $
��Back��6   $��32(�6(   $��80��66   $	��300��6B   $��0, 25, 0, 50q�6N   ��58      �� *GoToMenuCommand+  59      �� 5:      # [  6"   $
 Main�6D   5<      5=      5>      5?      5@      