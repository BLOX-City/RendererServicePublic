FROM ubuntu:24.10
RUN apt-get update && apt-get -y install aspnetcore-runtime-8.0 && apt-get -y install xvfb
RUN mkdir /home/bloxcity
RUN cp -r /etc/skel /home/bloxcity
RUN useradd -ms /bin/sh bloxcity
RUN chown -R bloxcity:bloxcity /home/bloxcity
USER bloxcity
RUN cd /home/bloxcity && mkdir renderer && mkdir renderer/assets
COPY bin/Release/net8.0/linux-x64/publish/ /home/bloxcity/renderer
COPY assets /home/bloxcity/renderer/assets
USER root
COPY entry.sh /home/bloxcity
RUN chmod +x /home/bloxcity/entry.sh

USER bloxcity
EXPOSE 1444/tcp
WORKDIR /home/bloxcity
ENTRYPOINT ./entry.sh
