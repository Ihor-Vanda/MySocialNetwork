function Message() {
  const name = "World";
  if (name) return <h1>Hello {name}</h1>;
  return <h1>Hellow React app</h1>;
}

export default Message;
