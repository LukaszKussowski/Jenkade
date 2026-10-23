# Recording Jenkins responses

`FakeJenkins` (`tests/Jenkade.Testing`) serves files from `tests/Jenkade.Testing/Recordings/`. Each file is one response as `curl -si` prints it: status line, headers, blank line, body. Record them from a real Jenkins on a machine that can reach one; the dev container can't.

The recordings from #5 were written by hand in Jenkins' format. Replace one with a real recording whenever you touch the operation it covers.

## Record

```bash
curl -si -u "$JENKINS_USER:$JENKINS_TOKEN" 'https://jenkins.example.org/api/json?tree=mode' > tests/Jenkade.Testing/Recordings/root.http
```

- Request exactly the URL the client sends (same `tree=` selector, same `start=` for `logText/progressiveText`): the response's shape depends on it.
- Record errors by provoking them: a wrong token for 401, a user without Overall/Read for 403, a missing Job for 404.
- Leave out `--compressed` and `-L`: the client never follows redirects, and the parser skips `Content-Length`, `Transfer-Encoding` and `Content-Encoding` because curl prints the body decoded.

## Clean up before committing

- Host → `jenkins.example.com` everywhere (body, `Location`, URLs inside JSON); `FakeJenkins` refuses every other host.
- User names → `anna`; real Job, Folder and branch names → neutral ones the tests can assert on.
- Drop `Set-Cookie`, `X-Jenkins-Session` values and anything else identifying the instance; API tokens and crumbs never appear in a recording.
- Name the file after what it shows (`unauthorized.http`, `job-with-parameters.http`); `Recordings/*.http` are embedded automatically.
