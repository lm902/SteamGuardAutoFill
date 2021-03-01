cd SteamGuardAutoFill/bin/Release
mv SteamGuardAutoFill.exe SteamGuardAutoFill.unsigned.exe
sign -n Steam令牌验证码自动填充 SteamGuardAutoFill.unsigned.exe SteamGuardAutoFill.exe
rm SteamGuardAutoFill.unsigned.exe