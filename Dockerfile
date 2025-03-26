FROM scottyhardy/docker-wine:stable-8.0.2

###############################################
MAINTAINER cs <562387089@qq.com>

EXPOSE 8080

ENV WINEARCH=win32 \
    WINEPREFIX=/wine32 \
    XVFB_SCREEN=0 \
    XVFB_RESOLUTION="320x240x8" \
    DISPLAY=":95" \
    DEBIAN_FRONTEND=noninteractive

WORKDIR /wine32

RUN apt-get update && \
    apt-get install -y xvfb x11-utils && \
    mkdir /tmp/.X11-unix && \
    chmod 1777 /tmp/.X11-unix && \
    dpkg --add-architecture i386 && \
    apt-get update && \
    apt-get remove -y python3-crypto 
    
COPY bash/auto_xvfb.sh /usr/bin/auto_xvfb
COPY libs/ /wine32/drive_c/libs
COPY bin/ /wine32/drive_c/bin
COPY bash/docker_entrypoint.sh /usr/bin/entrypoint

RUN [ ! -f /bin/sh ] || mv /bin/sh /bin/sh.old && ln -s /bin/bash /bin/sh
RUN test -f /usr/bin/auto_xvfb
RUN bash /usr/bin/auto_xvfb || echo "auto_xvfb 不存在，跳过"

RUN winetricks win8 nocrashdialog
#RUN wine /wine32/drive_c/libs/wine/dotnet46.exe /q
#RUN wine msiexec /i /wine32/drive_c/libs/wine/msxml3.msi /q
RUN winetricks --unattended win8 nocrashdialog msxml3 dotnet46



RUN cp /wine32/drive_c/libs/tini/tini /tini && \
    chmod +x /tini && \
    chmod +x /usr/bin/entrypoint

ENTRYPOINT ["/tini", "--", "/usr/bin/entrypoint"]
CMD ["--urls", "http://0.0.0.0:8080"]
