import React from 'react';

const HomePage = () => {
  const [values, setValues] = React.useState({
    email: '',
    password: '',
  });

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setValues(values=>({ ...values, [name]: value }));
  };

  const {email,password}=values;

  const handleSubmit = (event:React.FormEvent) => {
    event.preventDefault();

    fetch('https://localhost:5002/accounts/authenticate',{
      headers:{
        'Content-Type':'application/json',
        'accept':'application/json'
      },
      method: 'POST',
      body: JSON.stringify({email,password}),
    })
      .then((res) => res.json())
      .then((data) => {
        console.log(data);
      })
      .catch((err) => console.error(err));
  };

  return (
    <div>
      <form onSubmit={handleSubmit}>
      <div>
        <label htmlFor="email">Email</label>
        <input
          type="email"
          id="email"
          name='email'
          value={values.email}
          onChange={handleChange}
        />
      </div>
      <div>
        <label htmlFor="password">Password</label>
        <input
          type="password"
          id="password"
          name='password'
          value={values.password}
          onChange={handleChange}
        />
      </div>
      <button type='submit'>Submit</button></form>
    </div>
  );
};

export default HomePage;
