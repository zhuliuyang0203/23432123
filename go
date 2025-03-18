#!/usr/bin/env bash

# we want jruby-complete to take care of all things ruby
unset GEM_HOME
unset GEM_PATH

JAVA_OPTS="-client -Xmx4096m -XX:ReservedCodeCacheSize=512m -XX:MetaspaceSize=1024m --add-modules java.se --add-opens java.base/java.lang=ALL-UNNAMED --add-opens java.base/java.io=ALL-UNNAMED --add-opens java.base/java.lang.reflect=ALL-UNNAMED --add-opens java.base/javax.crypto=ALL-UNNAMED"

# The first argument is always the Rake task name
task="$1"

# Shift the task off and get the remaining arguments
shift

# Leave task alone if already passing in arguments the normal way
if [[ "$task" != *[*]* ]]; then
  # Combine remaining arguments into a single string, clean up spaces after commas, and replace spaces with commas
  args=$(IFS=' '; echo "$*" | sed -e 's/,[ ]*/,/g' -e 's/ /,/g')

# If there are any arguments, format them as task[arg1,arg2,...]
  if [ -n "$args" ]; then
    task="$task[$args]"
    echo "Executing rake task: $task"
  fi
fi

java $JAVA_OPTS -jar third_party/jruby/jruby-complete.jar -X-C -S rake $task

