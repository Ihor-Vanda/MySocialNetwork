interface Props {
  children: string;
  color?: "primary" | "secondary" | "danger";
  onClickButton: () => void;
}

const Button = ({ children, onClickButton, color = "primary" }: Props) => {
  return (
    <button className={"btn btn-" + color} onClick={onClickButton}>
      {children}
    </button>
  );
};

export default Button;
